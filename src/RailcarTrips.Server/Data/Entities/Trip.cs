using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailcarTrips.Server.Data.Entities;

/// <summary>
/// A processed railcar trip, bounded by a W (Released) event at origin
/// and a Z (Placed) event at destination.
/// </summary>
public class Trip
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string EquipmentId { get; set; } = string.Empty;

    public int OriginCityId { get; set; }

    [ForeignKey(nameof(OriginCityId))]
    public City? OriginCity { get; set; }

    public int DestinationCityId { get; set; }

    [ForeignKey(nameof(DestinationCityId))]
    public City? DestinationCity { get; set; }

    /// <summary>Start time in UTC (converted from origin city's local time).</summary>
    public DateTime StartUtc { get; set; }

    /// <summary>End time in UTC (converted from destination city's local time).</summary>
    public DateTime EndUtc { get; set; }

    /// <summary>Total duration of the trip in hours.</summary>
    public double TotalTripHours { get; set; }

    /// <summary>All events (W, A, D, Z) belonging to this trip.</summary>
    public List<EquipmentEvent> Events { get; set; } = new();
}
