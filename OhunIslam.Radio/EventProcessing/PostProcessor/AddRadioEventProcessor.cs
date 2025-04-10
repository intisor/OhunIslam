// ScopeFactory is required to be injected
// BackgroundServices are using ScopeFactory to perform a task in background
// This processor will be executed in a background service to add a post that comes from PostService
using OhunIslam.Radio.Services;
using OhunIslam.Shared.Models;

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
            var streamingStatus = DeserializeMessage(message);
            if (streamingStatus != null)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var rabbitMQService = scope.ServiceProvider.GetRequiredService<MassTransitService>();
                rabbitMQService.PublishStreamingStatus(streamingStatus);
                _logger.LogInformation(
                    "Successfully processed streaming status - Title: {MediaTitle}, Status: {Status}",
                    streamingStatus.MediaTitle,
                    streamingStatus.StreamStatus
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing radio event: {Message}", ex.Message);
            throw;
        }
    }

    private RadioStreamingStatus DeserializeMessage(string message)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<RadioStreamingStatus>(message);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Deserialization error: {Message}", ex.Message);
            return null;
        }
    }
}
