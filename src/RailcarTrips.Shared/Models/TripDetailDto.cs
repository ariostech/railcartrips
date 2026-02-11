namespace RailcarTrips.Shared.Models;

/// <summary>
/// Detail view combining a trip with its ordered events.
/// </summary>
public class TripDetailDto
{
    public TripDto Trip { get; set; } = new();
    public List<EventDto> Events { get; set; } = new();
}
