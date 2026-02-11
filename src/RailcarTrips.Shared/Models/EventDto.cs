namespace RailcarTrips.Shared.Models;

public class EventDto
{
    public int Id { get; set; }
    public string EquipmentId { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventTimeUtc { get; set; }
    public DateTime EventTimeLocal { get; set; }
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
}
