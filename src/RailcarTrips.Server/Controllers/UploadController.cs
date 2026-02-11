using Microsoft.AspNetCore.Mvc;
using RailcarTrips.Server.Services;
using RailcarTrips.Shared.Models;

namespace RailcarTrips.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly TripProcessingService _tripProcessingService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(TripProcessingService tripProcessingService, ILogger<UploadController> logger)
    {
        _tripProcessingService = tripProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Upload an equipment events CSV file to be processed into trips.
    /// </summary>
    [HttpPost("events")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<UploadResultDto>> UploadEvents([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new UploadResultDto { Success = false, ErrorMessage = "No file uploaded." });

        // TODO: Validate content type for additional security
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new UploadResultDto { Success = false, ErrorMessage = "Only CSV files are accepted." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _tripProcessingService.ProcessEventsFileAsync(stream);

            if (!result.Success)
                return UnprocessableEntity(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during file upload processing");
            return StatusCode(500, new UploadResultDto
            {
                Success = false,
                ErrorMessage = $"Server error: {ex.Message}"
            });
        }
    }
}
