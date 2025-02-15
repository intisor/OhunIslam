using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



// Add services for controllers
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
    UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
    VirtualHost = "/",
    Port = 5672
});
// Register IHttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddScoped<RabbitMQService>();
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// Add routing and endpoints for controllers
app.UseRouting();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/api/radio/play");
    }
    else
    {
        await next();
    }
});


app.Run();
//   "Container (Dockerfile)": {
//       "commandName": "Docker",
//       "launchBrowser": true,
//       "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
//       "environmentVariables": {
//         "ASPNETCORE_HTTPS_PORTS": "8081",
//         "ASPNETCORE_HTTP_PORTS": "8080"
//       },
//       "publishAllPorts": true,
//       "useSSL": true
//     }