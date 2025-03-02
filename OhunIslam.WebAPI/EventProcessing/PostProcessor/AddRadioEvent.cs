// ScopeFactory is required to be injected
// BackgroundServices are using ScopeFactory to perform a task in background
// This processor will be executed in a background service to add a post that comes from PostService
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Model;

public class AddRadioEvent
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AddRadioEvent(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ProcessAddRadioEvent(string message)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            try
            {
                var mediaContext = scope.ServiceProvider.GetRequiredService<MediaContext>();
                var radioItem = JsonSerializer.Deserialize<MediaItem>(message);
                
                if (radioItem != null)
                {
                    await mediaContext.MediaItem.AddAsync(radioItem);
                    await mediaContext.SaveChangesAsync();
                    Console.WriteLine($"Radio item {radioItem.MediaTitle} has been added to the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing radio message: {ex.Message}");
                throw;
            }
        }
    }
}
