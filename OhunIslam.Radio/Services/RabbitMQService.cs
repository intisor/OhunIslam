using System.Text;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;

internal class RabbitMQService : IDisposable
{
    private readonly Task<IConnection> _connection;
    private readonly Task<IChannel> _channel;

    public RabbitMQService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnectionAsync();
        _channel = _connection.Result.CreateChannelAsync();
        _channel.Result.QueueDeclareAsync("queue_radio", false, false, false, null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.Result.BasicPublishAsync("OhunIslam", "radio", body);
    } 

    public void Dispose()
    {
       _channel.Result.CloseAsync();
       _connection.Result.CloseAsync();
    }
}