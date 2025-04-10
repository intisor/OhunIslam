using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add this using directive
using OhunIslam.Radio.Services;
using OhunIslam.WebAPI.EventProcessing.PostProcessor;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Logging.AddProvider(new FileLoggerProvider("WebAPILogs.txt"));
builder.Services.AddDbContext<MediaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AddControllers();
// Configure RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(sp =>
    new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest"
    });
builder.Services.AddSingleton<RadioMessageSubscriber>();
builder.Services.AddHostedService<RadioMessageSubscriber>();
builder.Services.AddScoped<AddRadioEventProcessor>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MassTSConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("queue_stats", e =>
        {
            e.ConfigureConsumer<MassTSConsumer>(context);
            e.PrefetchCount = 1; // Process one message at a time
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10))); // Retry failed messages
        });
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
