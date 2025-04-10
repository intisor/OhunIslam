using MassTransit;
using OhunIslam.Shared.Models;
using OhunIslam.WebAPI.Infrastructure;

namespace OhunIslam.WebAPI.Services
{
    public class MassTSConsumer : IConsumer<RadioStreamingStatus>, IConsumer<StreamStatsUpdate>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MassTSConsumer> _logger;

        public MassTSConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<MassTSConsumer> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RadioStreamingStatus> context)
        {
            var streamingStatus = context.Message;
            _logger.LogInformation("[{Timestamp}] Received streaming status: {@Status}", DateTime.Now, streamingStatus);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MediaContext>();

                var consumedMessage = new OhunIslam.WebAPI.Model.ConsumedMessage
                {
                    MessageContent = System.Text.Json.JsonSerializer.Serialize(streamingStatus),
                    ReceivedAt = DateTime.Now,
                    MediaTitle = streamingStatus.MediaTitle,
                    StreamStartTime = streamingStatus.StreamStartTime,
                    StreamStatus = streamingStatus.StreamStatus,
                    StreamDuration = streamingStatus.StreamDuration
                };

                await dbContext.ConsumedMessages.AddAsync(consumedMessage);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("[{Timestamp}] Stored streaming status in database for: {MediaTitle}", DateTime.Now, streamingStatus.MediaTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Timestamp}] Error processing RadioStreamingStatus: {ErrorMessage}", DateTime.Now, ex.Message);
                throw;
            }
        }

        public async Task Consume(ConsumeContext<StreamStatsUpdate> context)
        {
            var statsUpdate = context.Message;
            _logger.LogInformation("[{Timestamp}] Received stats update: {@StatsUpdate}", DateTime.Now, statsUpdate);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MediaContext>();

                var streamStats = new OhunIslam.WebAPI.Model.StreamStats
                {
                    TotalStreamsToday = statsUpdate.TotalStreamsToday,
                    UpdateTime = statsUpdate.UpdateTime,
                    MessageContent = System.Text.Json.JsonSerializer.Serialize(statsUpdate),
                    ReceivedAt = DateTime.Now
                };
                await dbContext.StatsItems.AddAsync(streamStats);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("[{Timestamp}] Stored stats update in StreamStats: {TotalStreamsToday} streams today", DateTime.Now, statsUpdate.TotalStreamsToday);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Timestamp}] Error processing StreamStatsUpdate: {ErrorMessage}", DateTime.Now, ex.Message);
                throw;
            }
        }
    }
}
