using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


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

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve the audio stream.");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                _rabbitMQService.PublishToWebAPI($"Radio stream started at :-  {DateTime.Now}");

                return File(stream, "audio/mpeg");
            }
            catch (System.Exception ex)
            {
              return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}