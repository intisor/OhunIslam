using Microsoft.AspNetCore.Mvc;
using OhunIslam.Shared.Models;
using System.IO;
using System.Threading.Tasks;
namespace OhunIslam.Radio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RadioController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RabbitMQService _rabbitMQService;

        public RadioController(IHttpClientFactory httpClientFactory, RabbitMQService rabbitMQService)
        {
            _httpClientFactory = httpClientFactory;
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet("play")]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> Play()
        {
            string streamUrl = "https://go.webgateready.com/bondfm";
            Console.WriteLine("=== Radio Stream Connection Attempt ====");
            Console.WriteLine($"[{DateTime.Now}] Starting connection to stream");
            Console.WriteLine($"[{DateTime.Now}] Stream URL: {streamUrl}");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                // httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("=== Radio Stream Connection Failed ====");
                    Console.WriteLine($"[{DateTime.Now}] Connection failed with status code: {response.StatusCode}");
                    Console.WriteLine($"[{DateTime.Now}] Response reason phrase: {response.ReasonPhrase}");
                    return StatusCode((int)response.StatusCode, "Failed to verify the audio stream.");
                }

                Console.WriteLine("=== Radio Stream Connection Successful ====");
                Console.WriteLine($"[{DateTime.Now}] Successfully verified stream availability");
                Console.WriteLine($"[{DateTime.Now}] Content type: {response.Content.Headers.ContentType}");

                // var stream = await response.Content.ReadAsStreamAsync();

                _rabbitMQService.PublishStreamingStatus(new RadioStreamingStatus
                {
                    MediaTitle = "Bond FM Radio",
                    StreamStatus = "Playing",
                    StreamStartTime = DateTime.UtcNow
                });

                return Ok(new
                {
                    mediaTitle = "Bond FM Radio",
                    streamUrl = streamUrl,
                    status = "Available",
                    contentType = response.Content.Headers.ContentType?.ToString(),
                    message = "Stream is available. Use the streamUrl in an audio player to listen."
                });
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("=== Radio Stream Error ====");
                Console.WriteLine($"[{DateTime.Now}] Error type: {ex.GetType().Name}");
                Console.WriteLine($"[{DateTime.Now}] Error message: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now}] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[{DateTime.Now}] Inner exception: {ex.InnerException?.Message ?? "None"}");

                _rabbitMQService.PublishStreamingStatus(new RadioStreamingStatus
                {
                    MediaTitle = "Bond FM Radio",
                    StreamStatus = "Error",
                    StreamStartTime = DateTime.UtcNow
                });
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}