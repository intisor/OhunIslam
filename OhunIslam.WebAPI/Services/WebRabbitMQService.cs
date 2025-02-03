using System;
using System.Text;
using RabbitMQ.Client;

namespace OhunIslam.WebAPI.Services;

public class WebRabbitMQService : IDisposable
{

    private readonly Task<IConnection> _connection;
    private readonly Task<IChannel> _channel;

    public WebRabbitMQService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnectionAsync();
        _channel = _connection.Result.CreateChannelAsync();
        _channel.Result.QueueDeclareAsync("queue_webapi", false, false, false, null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.Result.BasicPublishAsync("OhunIslam", "webAPI", body);
    } 

    public void Dispose()
    {
       _channel.Result.CloseAsync();
       _connection.Result.CloseAsync();
    }
}
