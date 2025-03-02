using Microsoft.EntityFrameworkCore;
using OhunIslam.WebAPI.EventProcessing.PostProcessor;
using OhunIslam.WebAPI.Infrastructure;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
