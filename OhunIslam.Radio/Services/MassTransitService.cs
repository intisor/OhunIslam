using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OhunIslam.Shared.Models;
using RabbitMQ.Client;

namespace OhunIslam.Radio.Services
{
    public class MassTransitService : IDisposable
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MassTransitService> _logger;
        private readonly IBusControl _busControl; // Only used if not relying on DI
        private int _totalStreamsToday;
        private bool _disposed;

        public MassTransitService(
            IPublishEndpoint publishEndpoint,
            ILogger<MassTransitService> logger,
            string rabbitMqHost = "localhost")
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _totalStreamsToday = 0;

            // Optional: Manual bus config if not using DI-hosted MassTransit
            _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(2)));

                cfg.Publish<RadioStreamingStatus>(p =>
                {
                    p.Durable = true;
                    p.AutoDelete = false;
                });
                cfg.Publish<StreamStatsUpdate>(p =>
                {
                    p.Durable = true;
                    p.AutoDelete = false;
                });
            });

            // Start the bus if manually configured
            _busControl?.Start();
            _logger.LogInformation($"[{DateTime.Now}] MassTransitService initialized and connected to RabbitMQ at {rabbitMqHost}");
        }

        public async Task PublishStreamingStatus(RadioStreamingStatus status)
        {
            _logger.LogInformation("Publishing streaming status: {@Status}", status);
            try
            {
                var sendEndpoint = await _busControl.GetSendEndpoint(new Uri("exchange:radio_streaming_queue"));
                _logger.LogDebug("Send endpoint retrieved for radio_streaming_queue");
                await sendEndpoint.Send(status); // No routing key—fanout exchange
                _logger.LogInformation("Successfully published to radio_streaming_queue: {@Status}", status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing streaming status: {Message}", ex.Message);
                throw;
            }
        }
        public async Task PublishStatsUpdate(RadioStreamingStatus stream)
        {
            _logger.LogInformation($"[{DateTime.Now}] PublishStatsUpdate called for {stream.MediaTitle}");
            if (stream.StreamStatus == StreamStatus.Started)
            {
                _totalStreamsToday++;
                var statsUpdate = new StreamStatsUpdate
                {
                    TotalStreamsToday = _totalStreamsToday,
                    UpdateTime = DateTime.UtcNow,
                };
                _logger.LogInformation("Publishing stats update: {@StatsUpdate}", statsUpdate);
                try
                {
                    var sendEndpoint = await _busControl.GetSendEndpoint(new Uri("exchange:radio_streaming_queue"));
                    _logger.LogDebug("Send endpoint retrieved for radio_streaming_queue");
                    await sendEndpoint.Send(statsUpdate);
                    _logger.LogInformation($"[{DateTime.Now}] Published stats update to radio_streaming_queue: {statsUpdate.TotalStreamsToday} streams today");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing stats update: {@StatsUpdate}", statsUpdate);
                    throw;
                }
            }
        }
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _busControl?.Stop(); // Only stop if initialized
                _logger.LogInformation("MassTransit bus stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}