using MassTransit;
using OhunIslam.Radio.Services;
using OhunIslam.Shared.Models;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.AddProvider(new FileLoggerProvider("RadioLogs.txt")); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
//builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
//{
//    HostName = "localhost",
//    Port = 5672,
//    UserName = "guest",
//    Password = "guest"
//});
//builder.Services.AddSingleton<MassTransitService>();

//builder.Services.AddSingleton<AddRadioEventProcessor>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((MTcofig, RMconfig) =>
    {
        RMconfig.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        RMconfig.Message<StreamStatsUpdate>(e => e.SetEntityName("radio_exchange"));
        RMconfig.Publish<StreamStatsUpdate>(e =>
        {
            e.ExchangeType = ExchangeType.Direct;
        });
        RMconfig.ReceiveEndpoint("radio_streaming_queue", e =>
        {
            //e.ConfigureConsumeTopology = false;
            e.Durable = true;
            e.AutoDelete = false;
            //e.Bind("radio_exchange", b =>
            //{
            //    b.RoutingKey = "streaming.status";
            //    b.Durable = true;
            //    b.AutoDelete = false;
            //    b.ExchangeType = ExchangeType.Direct;
            //});
        });
        RMconfig.Publish<RadioStreamingStatus>(e =>
        {
            e.ExchangeType = ExchangeType.Direct;
        });
        //RMconfig.Publish<StreamStatsUpdate>(e => e.ExchangeType = ExchangeType.Direct);
    });
    x.AddRequestClient<StreamStatsUpdate>();
});

builder.Services.AddScoped<MassTransitService>();

// Register IHttpClientFactory
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add routing and endpoints for controllers
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/", async context =>
    {
        context.Response.Redirect("/api/radio/play");
    });
});

app.Run();
