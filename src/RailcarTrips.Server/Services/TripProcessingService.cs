using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RailcarTrips.Server.Data;
using RailcarTrips.Server.Data.Entities;
using RailcarTrips.Shared.Models;
using System.Globalization;

namespace RailcarTrips.Server.Services;

/// <summary>
/// Service responsible for parsing the uploaded equipment events CSV,
/// converting local times to UTC, and processing events into trips.
/// </summary>
public class TripProcessingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TripProcessingService> _logger;

    public TripProcessingService(AppDbContext db, ILogger<TripProcessingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Parse and process an uploaded equipment events CSV stream into trips.
    /// </summary>
    public async Task<UploadResultDto> ProcessEventsFileAsync(Stream csvStream)
    {
        var result = new UploadResultDto();

        try
        {
            var rawEvents = ParseCsv(csvStream);
            _logger.LogInformation("Parsed {Count} raw events from CSV", rawEvents.Count);

            var cities = await _db.Cities
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id);

            var equipmentEvents = new List<EquipmentEvent>();
            int skippedEvents = 0;
            foreach (var raw in rawEvents)
            {
                try
                {
                    if (!cities.TryGetValue(raw.CityId, out var city))
                    {
                        _logger.LogWarning("Unknown City Id {CityId} for event {EquipmentId}/{EventCode}",
                            raw.CityId, raw.EquipmentId, raw.EventCode);
                        skippedEvents++;
                        continue;
                    }

                    var tz = TimeZoneInfo.FindSystemTimeZoneById(city.TimeZone);
                    var localDt = DateTime.SpecifyKind(raw.EventTime, DateTimeKind.Unspecified);

                    DateTime adjustedDt = localDt;
                    if (tz.IsInvalidTime(localDt))
                    {
                        var adjustment = tz.GetAdjustmentRules()
                            .FirstOrDefault(r => localDt >= r.DateStart && localDt <= r.DateEnd);
                        if (adjustment != null)
                        {
                            adjustedDt = localDt.Add(adjustment.DaylightDelta);
                        }
                        else
                        {
                            adjustedDt = localDt.AddHours(1);
                        }
                        _logger.LogWarning(
                            "Invalid local time {LocalTime} in {TimeZone} (DST gap) for {EquipmentId}/{EventCode}. Adjusted to {AdjustedTime}.",
                            localDt, city.TimeZone, raw.EquipmentId, raw.EventCode, adjustedDt);
                    }

                    var utcDt = TimeZoneInfo.ConvertTimeToUtc(adjustedDt, tz);

                    equipmentEvents.Add(new EquipmentEvent
                    {
                        EquipmentId = raw.EquipmentId.Trim(),
                        EventCode = raw.EventCode.Trim().ToUpperInvariant(),
                        EventTimeLocal = raw.EventTime,
                        EventTimeUtc = utcDt,
                        CityId = raw.CityId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping event {EquipmentId}/{EventCode} at {EventTime} — failed to process",
                        raw.EquipmentId, raw.EventCode, raw.EventTime);
                    skippedEvents++;
                }
            }

            var groupedByEquipment = equipmentEvents
                .GroupBy(e => e.EquipmentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => e.EventTimeUtc).ToList());

            var trips = new List<Trip>();
            int orphanedCount = 0;

            foreach (var (equipmentId, events) in groupedByEquipment)
            {
                var (equipmentTrips, orphaned) = BuildTripsForEquipment(equipmentId, events, cities);
                trips.AddRange(equipmentTrips);
                orphanedCount += orphaned;
            }

            // TODO: In production, consider a more granular approach — e.g. only replace
            // data for the equipment IDs present in the uploaded file, or support incremental uploads.
            _db.EquipmentEvents.RemoveRange(_db.EquipmentEvents);
            _db.Trips.RemoveRange(_db.Trips);
            await _db.SaveChangesAsync();

            _db.Trips.AddRange(trips);
            await _db.SaveChangesAsync();

            result.Success = true;
            result.TripsCreated = trips.Count;
            result.EventsProcessed = equipmentEvents.Count;
            result.OrphanedEvents = orphanedCount + skippedEvents;

            _logger.LogInformation(
                "Processed {Events} events into {Trips} trips ({Orphaned} orphaned events, {Skipped} skipped due to errors)",
                result.EventsProcessed, result.TripsCreated, orphanedCount, skippedEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing equipment events file");
            result.Success = false;
            result.ErrorMessage = $"Error processing file: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Parse CSV file into raw event records using CsvHelper.
    /// </summary>
    private List<CsvEquipmentEvent> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CsvEquipmentEventMap>();
        return csv.GetRecords<CsvEquipmentEvent>().ToList();
    }

    /// <summary>
    /// Walk through a single equipment's sorted events and assemble trips.
    /// 
    /// Business rules:
    ///   - W (Released) starts a new trip. Origin city = W event's city. Start UTC = W event's UTC time.
    ///   - A (Arrived) and D (Departed) are intermediate events attached to the current open trip.
    ///   - Z (Placed) ends the current trip. Destination city = Z event's city. End UTC = Z event's UTC time.
    ///   - Events occurring before the first W or after a Z (without a subsequent W) are considered orphaned.
    /// 
    /// Assumptions:
    ///   - If a second W is encountered while a trip is already open, the current trip is closed
    ///     as incomplete (destination/end are set from the last intermediate event, or left as origin)
    ///     and a new trip begins. This handles data anomalies gracefully.
    ///   - If the file ends with an open trip (no closing Z), it is saved as incomplete.
    /// </summary>
    private (List<Trip> trips, int orphanedCount) BuildTripsForEquipment(
        string equipmentId,
        List<EquipmentEvent> sortedEvents,
        Dictionary<int, City> cities)
    {
        var trips = new List<Trip>();
        Trip? currentTrip = null;
        int orphanedCount = 0;

        foreach (var evt in sortedEvents)
        {
            switch (evt.EventCode)
            {
                case "W":
                    if (currentTrip != null)
                    {
                        CloseIncompleteTrip(currentTrip);
                        trips.Add(currentTrip);
                        _logger.LogWarning(
                            "Equipment {EquipmentId}: New W event encountered while trip was still open. " +
                            "Previous trip closed as incomplete.", equipmentId);
                    }

                    currentTrip = new Trip
                    {
                        EquipmentId = equipmentId,
                        OriginCityId = evt.CityId,
                        StartUtc = evt.EventTimeUtc,
                        Events = new List<EquipmentEvent> { evt }
                    };
                    evt.Trip = currentTrip;
                    break;

                case "A":
                case "D":
                    if (currentTrip == null)
                    {
                        orphanedCount++;
                        _logger.LogDebug(
                            "Equipment {EquipmentId}: Orphaned {EventCode} event at {Time} (no open trip)",
                            equipmentId, evt.EventCode, evt.EventTimeUtc);
                    }
                    else
                    {
                        currentTrip.Events.Add(evt);
                        evt.Trip = currentTrip;
                    }
                    break;

                case "Z":
                    if (currentTrip == null)
                    {
                        orphanedCount++;
                        _logger.LogDebug(
                            "Equipment {EquipmentId}: Orphaned Z event at {Time} (no open trip)",
                            equipmentId, evt.EventTimeUtc);
                    }
                    else
                    {
                        currentTrip.DestinationCityId = evt.CityId;
                        currentTrip.EndUtc = evt.EventTimeUtc;
                        currentTrip.TotalTripHours = (currentTrip.EndUtc - currentTrip.StartUtc).TotalHours;
                        currentTrip.Events.Add(evt);
                        evt.Trip = currentTrip;
                        trips.Add(currentTrip);
                        currentTrip = null;
                    }
                    break;

                default:
                    _logger.LogWarning("Equipment {EquipmentId}: Unknown event code '{Code}'",
                        equipmentId, evt.EventCode);
                    orphanedCount++;
                    break;
            }
        }

        if (currentTrip != null)
        {
            CloseIncompleteTrip(currentTrip);
            trips.Add(currentTrip);
            _logger.LogInformation(
                "Equipment {EquipmentId}: Trip still open at end of events — saved as incomplete",
                equipmentId);
        }

        return (trips, orphanedCount);
    }

    /// <summary>
    /// Close an incomplete trip by using the last event's city/time as the destination/end.
    /// TODO: Consider flagging incomplete trips with a status field so the UI can distinguish them.
    /// </summary>
    private static void CloseIncompleteTrip(Trip trip)
    {
        var lastEvent = trip.Events.LastOrDefault();
        if (lastEvent != null)
        {
            trip.DestinationCityId = lastEvent.CityId;
            trip.EndUtc = lastEvent.EventTimeUtc;
        }
        else
        {
            trip.DestinationCityId = trip.OriginCityId;
            trip.EndUtc = trip.StartUtc;
        }
        trip.TotalTripHours = (trip.EndUtc - trip.StartUtc).TotalHours;
    }
}
