using System.ComponentModel.DataAnnotations;

namespace RailcarTrips.Server.Data.Entities;

/// <summary>
/// Represents a Canadian city with its time zone.
/// </summary>
public class City
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Windows time zone identifier (e.g. "Eastern Standard Time").
    /// Used to convert local event times to UTC.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TimeZone { get; set; } = string.Empty;
}
