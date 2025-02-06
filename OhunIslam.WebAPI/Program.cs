using Microsoft.EntityFrameworkCore;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
// mysql - SPoDUPw
// MsSQL - SDUP
// Add services to the container.
builder.Services.AddDbContext<MediaContext>(option =>
           option.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:HostName"] ?? "rabbitmq",
    UserName = builder.Configuration["RabbitMQ:UserName"] ?? "admin",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "admin",
    ConsumerDispatchConcurrency = 1
});
builder.Services.AddSingleton<WebRabbitMQService>();
builder.Services.AddHostedService<WebRabbitMQService>();

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
