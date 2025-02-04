using System.Text;
using System;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        _channel.Result.QueueDeclareAsync("queue_WebAPI", false, false, false, null);
    }

    public void PublishToWebAPI(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.Result.BasicPublishAsync("OhunIslam", "radio", body);
        System.Console.WriteLine($"Sent {message}");
    } 

    public async void  ConsumeFromWebAPI()
    {
        var consumer = new AsyncEventingBasicConsumer(await _channel);
        consumer.ReceivedAsync += (ConsumerObj, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body); 
            System.Console.WriteLine($"Received {message} at {DateTime.Now}");
            return Task.CompletedTask;
        };

        _channel.Result.BasicConsumeAsync("queue_WebAPI",true, consumer);
    } 
    public void Dispose()
    {
       _channel.Result.CloseAsync();
       _connection.Result.CloseAsync();
    }
}