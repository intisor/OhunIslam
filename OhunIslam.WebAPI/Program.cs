using Microsoft.EntityFrameworkCore;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MediaContext>(option =>
           option.UseMySql(builder.Configuration.GetConnectionString("ConnectionString"),
           new MySqlServerVersion(new Version(8, 0,1))));
builder.Services.AddControllers();
builder.Services.AddSingleton<WebRabbitMQService>();
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
