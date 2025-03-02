namespace OhunIslam.Shared.Models
{
    public class RadioStreamingStatus 
    {
        public string MediaTitle { get; set; }
        public DateTime StreamStartTime { get; set; }
        public string StreamStatus { get; set; } // e.g., "Started", "Stopped"
        public TimeSpan StreamDuration { get; set; }
    }
}