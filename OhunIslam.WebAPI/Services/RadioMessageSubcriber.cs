using MassTransit;
using OhunIslam.WebAPI.Services;

namespace OhunIslam.Radio.Services
{
    public class RadioMessageSubscriber : BackgroundService
    {
        private readonly IBusControl _busControl;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MassTSConsumer> _logger;
        private readonly IConfiguration _configuration;

        public RadioMessageSubscriber(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MassTSConsumer> logger)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            // Configure MassTransit bus
            _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(_configuration["RabbitMQ:Host"] ?? "localhost", h =>
                {
                    h.Username(_configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(_configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10))); // Retry every 10s, 5 attempts

                cfg.ReceiveEndpoint("radio_streaming_queue", e =>
                {
                    e.Durable = true;
                    e.AutoDelete = false;
                    e.Consumer(() => new MassTSConsumer(serviceScopeFactory, logger));
                });
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"[{DateTime.Now}] Starting RadioMessageSubscriber service");
            try
            {
                await _busControl.StartAsync(stoppingToken);
                _logger.LogInformation($"[{DateTime.Now}] MassTransit bus started, consuming from radio_streaming_queue");
                await Task.Delay(Timeout.Infinite, stoppingToken); // Keep running until stopped
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DateTime.Now}] Error starting MassTransit bus: {ex.Message}");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping RadioMessageSubscriber");
            try
            {
                await _busControl.StopAsync(stoppingToken);
                _logger.LogInformation("MassTransit bus stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during shutdown: {ex.Message}");
            }
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _busControl?.Stop();
            base.Dispose();
        }
    }

}