using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using OhunIslam.WebAPI.EventProcessing.PostProcessor;
using OhunIslam.WebAPI.Infrastructure;

public class RadioMessageSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RadioMessageSubscriber> _logger;
    private IConnection _connection;
    private IModel _channel;
    private const string ExchangeName = "OhunIslam";
    private const string QueueName = "queue_radio";
    private const string RoutingKey = "RadioKey";
    private const int RetryIntervalSeconds = 10;

    public RadioMessageSubscriber(
        IConfiguration configuration, 
        IConnectionFactory connectionFactory, 
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RadioMessageSubscriber> logger)
    {
        _configuration = configuration;
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    private bool TryConnectToRabbitMQ()
    {
        try
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            {
                return true;
            }

            // Clean up existing connections if any
            _channel?.Dispose();
            _connection?.Dispose();

            _logger.LogInformation("Attempting to connect to RabbitMQ...");
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
            
            _logger.LogInformation("Successfully connected to RabbitMQ");
            return true;
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogWarning($"Failed to connect to RabbitMQ: {ex.Message}. Will retry in {RetryIntervalSeconds} seconds.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error connecting to RabbitMQ: {ex.Message}");
            return false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"[{DateTime.Now}] Starting RadioMessageSubscriber service");
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!TryConnectToRabbitMQ())
            {
                _logger.LogWarning($"[{DateTime.Now}] Failed to connect to RabbitMQ. Retrying in {RetryIntervalSeconds} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), stoppingToken);
                continue;
            }

            try
            {
                _logger.LogInformation($"[{DateTime.Now}] Setting up message consumer for queue: {QueueName}");
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (sender, eventArgs) =>
                {
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($"[{DateTime.Now}] Received message: {message}");
                        _logger.LogInformation($"[{DateTime.Now}] Message properties - Exchange: {eventArgs.Exchange}, RoutingKey: {eventArgs.RoutingKey}");

                        using var scope = _serviceScopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<MediaContext>();

                        var consumedMessage = new OhunIslam.WebAPI.Model.ConsumedMessage
                        {
                            MessageContent = message,
                            ReceivedAt = DateTime.Now
                        };
                        await dbContext.ConsumedMessages.AddAsync(consumedMessage);
                        await dbContext.SaveChangesAsync();
                        var streamingStatus = System.Text.Json.JsonSerializer.Deserialize<OhunIslam.Shared.Models.RadioStreamingStatus>(message);

                        var mediaItem = new OhunIslam.WebAPI.Model.MediaItem
                        {
                            MediaTitle = streamingStatus.MediaTitle,
                            DateIssued = streamingStatus.StreamStartTime,
                            MediaDescription = $"Stream {streamingStatus.StreamStatus} at {streamingStatus.StreamStartTime}, Duration: {streamingStatus.StreamDuration}"
                        };

                        await dbContext.MediaItem.AddAsync(mediaItem);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"[{DateTime.Now}] Stored streaming status in database for: {streamingStatus.MediaTitle}");

                        var processor = scope.ServiceProvider.GetRequiredService<AddRadioEventProcessor>();
                        await processor.ProcessAddRadioEvent(message);

                        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                        _logger.LogInformation($"[{DateTime.Now}] Successfully processed and acknowledged message");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[{DateTime.Now}] Error processing message: {ex.Message}");
                        _logger.LogError($"[{DateTime.Now}] Stack trace: {ex.StackTrace}");
                        // Reject the message and requeue it
                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _logger.LogInformation($"[{DateTime.Now}] Starting to consume messages from queue: {QueueName}");
                _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{DateTime.Now}] Error in message consumer: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), stoppingToken);
            }
        }
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Stopping RabbitMQ subscriber");
            _channel?.Close();
            _connection?.Close();
            await base.StopAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during shutdown: {ex.Message}");
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}