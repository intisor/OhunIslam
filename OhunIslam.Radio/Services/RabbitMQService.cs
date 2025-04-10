//using System.Text;
//using RabbitMQ.Client;
//using System.Text.Json;
//using OhunIslam.Shared.Models;

//public class RabbitMQService : IDisposable
//{
//    private readonly IConnection _connection;
//    private readonly IModel _channel;
//    private const string ExchangeName = "OhunIslam";
//    private const string QueueName = "queue_radio";
//    private const string RoutingKey = "RadioKey";

//    private const int MaxRetries = 5;
//    private const int RetryDelayMs = 2000;

//    public RabbitMQService(IConnectionFactory connectionFactory)
//    {
//        int retryCount = 0;
//        while (retryCount < MaxRetries)
//        {
//            try
//            {
//                _connection = connectionFactory.CreateConnection();
//                _channel = _connection.CreateModel();

//                // Declare exchange and queue with durability
//                _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
//                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
//                _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

//                // Enable publisher confirms
//                _channel.ConfirmSelect();

//                // Setup error handling
//                SetupErrorHandling();

//                Console.WriteLine("Successfully connected to RabbitMQ");
//                return;
//            }
//            catch (Exception ex)
//            {
//                retryCount++;
//                Console.WriteLine($"Failed to connect to RabbitMQ (Attempt {retryCount}/{MaxRetries}): {ex.Message}");

//                if (retryCount < MaxRetries)
//                {
//                    Thread.Sleep(RetryDelayMs);
//                }
//                else
//                {
//                    throw new Exception($"Failed to connect to RabbitMQ after {MaxRetries} attempts", ex);
//                }
//            }
//        }
//    }

//    public async Task PublishStreamingStatus(RadioStreamingStatus status)
//    {
//        try
//        {
//            Console.WriteLine($"[{DateTime.Now}] Attempting to publish streaming status");
//            Console.WriteLine($"[{DateTime.Now}] Message details: Title={status.MediaTitle}, Status={status.StreamStatus}, Time={status.StreamStartTime}");
            
//            var message = JsonSerializer.Serialize(status);
//            var body = Encoding.UTF8.GetBytes(message);
//            var properties = _channel.CreateBasicProperties();
//            properties.Persistent = true; // Make message persistent
//            properties.ContentType = "application/json";

//            Console.WriteLine($"[{DateTime.Now}] Publishing message to exchange: {ExchangeName} with routing key: {RoutingKey}");
//            _channel.BasicPublish(ExchangeName, RoutingKey, properties, body);
//            _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
//            Console.WriteLine($"[{DateTime.Now}] Successfully published and confirmed streaming status for: {status.MediaTitle} - {status.StreamStatus}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"[{DateTime.Now}] Error publishing message: {ex.Message}");
//            Console.WriteLine($"[{DateTime.Now}] Stack trace: {ex.StackTrace}");
//            throw;
//        }
//    }

//    private void SetupErrorHandling()
//    {
//        _channel.CallbackException += (sender, ea) =>
//        {
//            Console.WriteLine($"Channel callback exception: {ea.Exception.Message}");
//        };

//        _connection.ConnectionShutdown += (sender, ea) =>
//        {
//            Console.WriteLine($"RabbitMQ connection shut down: {ea.ReplyText}");
//        };
//    }

//    public void Dispose()
//    {
//        try
//        {
//            _channel?.Close();
//            _connection?.Close();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error during disposal: {ex.Message}");
//        }
//    }
//}



