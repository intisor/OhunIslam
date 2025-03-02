using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.Shared.Models;

namespace OhunIslam.WebAPI.EventProcessing.PostProcessor
{
    public class AddRadioEventProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AddRadioEventProcessor> _logger;

        public AddRadioEventProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<AddRadioEventProcessor> logger)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessAddRadioEvent(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("Received empty or null message");
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                var streamingStatus = JsonSerializer.Deserialize<RadioStreamingStatus>(message);
                
                if (streamingStatus == null)
                {
                    _logger.LogError("Failed to deserialize message to RadioStreamingStatus");
                    throw new JsonException("Failed to deserialize message to RadioStreamingStatus");
                }

                _logger.LogInformation(
                    "Radio streaming status update received - Title: {MediaTitle}, Status: {Status}, Time: {StreamStartTime}",
                    streamingStatus.MediaTitle,
                    streamingStatus.StreamStatus,
                    streamingStatus.StreamStartTime
                );

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<MediaContext>();

                    var mediaItem = await context.MediaItem
                        .FirstOrDefaultAsync(m => m.MediaTitle == streamingStatus.MediaTitle);

                    if (mediaItem == null)
                    {
                        _logger.LogWarning("Media item not found in database: {MediaTitle}", streamingStatus.MediaTitle);
                        return;
                    }

                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation(
                        "Successfully processed streaming status for media: {MediaTitle}",
                        mediaItem.MediaTitle
                    );
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message: {Message}", message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing radio message: {Message}", ex.Message);
                throw;
            }
        }
    }
}