using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailcarTrips.Server.Data.Entities;

/// <summary>
/// An individual equipment event parsed from the uploaded CSV.
/// </summary>
public class EquipmentEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string EquipmentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(1)]
    public string EventCode { get; set; } = string.Empty;

    /// <summary>
    /// The original local date/time as provided in the CSV.
    /// Stored for reference; the authoritative time for ordering is EventTimeUtc.
    /// </summary>
    public DateTime EventTimeLocal { get; set; }

    /// <summary>
    /// The event time converted to UTC using the city's time zone.
    /// Used for chronological ordering across time zones.
    /// </summary>
    public DateTime EventTimeUtc { get; set; }

    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    /// <summary>
    /// FK to the parent trip. Null for orphaned events (before first W or after Z without a new W).
    /// </summary>
    public int? TripId { get; set; }

    [ForeignKey(nameof(TripId))]
    public Trip? Trip { get; set; }
}
