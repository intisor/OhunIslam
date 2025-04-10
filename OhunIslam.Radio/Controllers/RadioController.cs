using Microsoft.AspNetCore.Mvc;
using OhunIslam.Radio.Services;
using OhunIslam.Shared.Models;
namespace OhunIslam.Radio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RadioController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly RabbitMQService _rabbitMQService;
        private readonly MassTransitService _massTransitService;
        private readonly ILogger<RadioController> _logger;

        public RadioController(IHttpClientFactory httpClientFactory,ILogger<RadioController> logger, MassTransitService massTransitService)
        {
            _httpClientFactory = httpClientFactory;
            //_rabbitMQService = rabbitMQService;
            _logger = logger;
            _massTransitService = massTransitService;
        }

        [HttpGet("play")]
        public async Task<IActionResult> Play()
        {
            string streamUrl = "https://go.webgateready.com/bondfm";
            _logger.LogInformation("=== Radio Stream Connection Attempt ====");
            _logger.LogInformation($"[{DateTime.Now}] Starting connection to stream");
            _logger.LogInformation($"[{DateTime.Now}] Stream URL: {streamUrl}");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("=== Radio Stream Connection Failed ====");
                    _logger.LogError($"[{DateTime.Now}] Connection failed with status code: {response.StatusCode}");
                    _logger.LogError($"[{DateTime.Now}] Response reason phrase: {response.ReasonPhrase}");
                    return StatusCode((int)response.StatusCode, "Failed to verify the audio stream.");
                }

                _logger.LogInformation("=== Radio Stream Connection Successful ====");
                _logger.LogInformation($"[{DateTime.Now}] Successfully verified stream availability");
                _logger.LogInformation($"[{DateTime.Now}] Content type: {response.Content.Headers.ContentType}");

                var streamingStatus = new RadioStreamingStatus
                {
                    MediaTitle = "Bond FM Radio",
                    StreamStatus = StreamStatus.Started,
                    StreamStartTime = DateTime.UtcNow,
                    StreamDuration = TimeSpan.Zero
                };

                _logger.LogInformation("Publishing streaming status: {@StreamingStatus}", streamingStatus);
                await _massTransitService.PublishStreamingStatus(streamingStatus);
                _logger.LogInformation("Successfully published streaming status");

                await _massTransitService.PublishStatsUpdate(streamingStatus);
                _logger.LogInformation("Successfully published stats update");

                return Ok(new
                {
                    mediaTitle = "Bond FM Radio",
                    streamUrl = streamUrl,
                    status = "Started",
                    contentType = response.Content.Headers.ContentType?.ToString(),
                    message = "Stream is available. Use the streamUrl in an audio player to listen."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("=== Radio Stream Error ====");
                _logger.LogError($"[{DateTime.Now}] Error type: {ex.GetType().Name}");
                _logger.LogError($"[{DateTime.Now}] Error message: {ex.Message}");
                _logger.LogError($"[{DateTime.Now}] Stack trace: {ex.StackTrace}");
                _logger.LogError($"[{DateTime.Now}] Inner exception: {ex.InnerException?.Message ?? "None"}");

                var errorStatus = new RadioStreamingStatus
                {
                    MediaTitle = "Bond FM Radio",
                    StreamStatus = StreamStatus.Error,
                    StreamStartTime = DateTime.UtcNow
                };

                _logger.LogInformation("Publishing error streaming status: {@ErrorStatus}", errorStatus);
                await _massTransitService.PublishStreamingStatus(errorStatus);
                _logger.LogInformation("Successfully published error streaming status");

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}