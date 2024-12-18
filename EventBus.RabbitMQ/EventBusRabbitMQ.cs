using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EventBus.RabbitMQ
{
    internal class EventBusRabbitMQ : Eventbus, IDisposable
    {
        private readonly Task<IConnection> _connection;
        private Task<IChannel> _channel;
        private readonly string _queueName;

        public EventBusRabbitMQ(string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,             // Default RabbitMQ port
                UserName = "guest",      // Default username
                Password = "guest",      // Default password
                VirtualHost = "/",
            };
           Task<IConnection> connection = factory.CreateConnectionAsync();
            connection.Wait();
            _connection = connection;
            _channel =  connection.Result.CreateChannelAsync();
            _queueName = queueName;
            _channel.Result.QueueDeclareAsync(_queueName, false, false, false, null);
        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.Result.BasicPublishAsync("", _queueName, body);
        }

        public override void Subscribe<T, TH>()
        {
            base.Subscribe<T, TH>();
            IChannel channel = (IChannel)_channel;
            var eventName = typeof(T).Name;
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                await ProcessEvent(eventName, message);
            };

            _channel.Result.BasicConsumeAsync(_queueName, true, consumer);
        }

        public void Dispose()
        {
            _channel.Result.CloseAsync();
            _connection.Result.CloseAsync();
        }
    }
}
