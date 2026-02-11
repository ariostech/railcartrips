namespace RailcarTrips.Shared.Models;

/// <summary>
/// Response returned after uploading and processing equipment events.
/// </summary>
public class UploadResultDto
{
    public bool Success { get; set; }
    public int TripsCreated { get; set; }
    public int EventsProcessed { get; set; }
    public int OrphanedEvents { get; set; }
    public string? ErrorMessage { get; set; }
}
