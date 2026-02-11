namespace RailcarTrips.Shared.Models;

/// <summary>
/// Data transfer object representing a processed railcar trip.
/// </summary>
public class TripDto
{
    public int Id { get; set; }
    public string EquipmentId { get; set; } = string.Empty;
    public int OriginCityId { get; set; }
    public string OriginCityName { get; set; } = string.Empty;
    public int DestinationCityId { get; set; }
    public string DestinationCityName { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public double TotalTripHours { get; set; }
}
