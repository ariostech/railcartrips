using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailcarTrips.Server.Data;
using RailcarTrips.Shared.Models;

namespace RailcarTrips.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TripsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all processed trips with city names.
    /// </summary>
    /// <remarks>
    /// TODO: Add pagination, filtering (by equipment id, date range), and sorting
    /// for production use. Current implementation returns all trips which is fine
    /// for the sample data set but would not scale.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<List<TripDto>>> GetTrips()
    {
        var trips = await _db.Trips
            .Include(t => t.OriginCity)
            .Include(t => t.DestinationCity)
            .OrderBy(t => t.EquipmentId)
            .ThenBy(t => t.StartUtc)
            .Select(t => new TripDto
            {
                Id = t.Id,
                EquipmentId = t.EquipmentId,
                OriginCityId = t.OriginCityId,
                OriginCityName = t.OriginCity != null ? t.OriginCity.Name : "Unknown",
                DestinationCityId = t.DestinationCityId,
                DestinationCityName = t.DestinationCity != null ? t.DestinationCity.Name : "Unknown",
                StartUtc = t.StartUtc,
                EndUtc = t.EndUtc,
                TotalTripHours = Math.Round(t.TotalTripHours, 2)
            })
            .ToListAsync();

        return Ok(trips);
    }

    /// <summary>
    /// Get a single trip with its ordered events (bonus feature).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TripDetailDto>> GetTripDetail(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.OriginCity)
            .Include(t => t.DestinationCity)
            .Include(t => t.Events)
                .ThenInclude(e => e.City)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
            return NotFound();

        var detail = new TripDetailDto
        {
            Trip = new TripDto
            {
                Id = trip.Id,
                EquipmentId = trip.EquipmentId,
                OriginCityId = trip.OriginCityId,
                OriginCityName = trip.OriginCity?.Name ?? "Unknown",
                DestinationCityId = trip.DestinationCityId,
                DestinationCityName = trip.DestinationCity?.Name ?? "Unknown",
                StartUtc = trip.StartUtc,
                EndUtc = trip.EndUtc,
                TotalTripHours = Math.Round(trip.TotalTripHours, 2)
            },
            Events = trip.Events
                .OrderBy(e => e.EventTimeUtc)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    EquipmentId = e.EquipmentId,
                    EventCode = e.EventCode,
                    EventDescription = GetEventDescription(e.EventCode),
                    EventTimeUtc = e.EventTimeUtc,
                    EventTimeLocal = e.EventTimeLocal,
                    CityId = e.CityId,
                    CityName = e.City?.Name ?? "Unknown"
                })
                .ToList()
        };

        return Ok(detail);
    }

    /// <summary>
    /// Helper to get a human-readable event description.
    /// TODO: Could load from EventCodeDefinitions table instead of hardcoding.
    /// </summary>
    private static string GetEventDescription(string code) => code switch
    {
        "W" => "Released",
        "A" => "Arrived",
        "D" => "Departed",
        "Z" => "Placed",
        _ => "Unknown"
    };
}
