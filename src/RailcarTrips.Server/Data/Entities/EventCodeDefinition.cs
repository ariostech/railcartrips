using System.ComponentModel.DataAnnotations;

namespace RailcarTrips.Server.Data.Entities;

/// <summary>
/// Lookup table for event codes (W, A, D, Z).
/// </summary>
public class EventCodeDefinition
{
    [Key]
    [MaxLength(1)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string LongDescription { get; set; } = string.Empty;
}
