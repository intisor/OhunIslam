namespace OhunIslam.Shared.Models
{
    public enum StreamStatus
    {
        Started,
        Stopped,
        Error,
        Playing
    }

    public class RadioStreamingStatus
    {
        public string? MediaTitle { get; set; }
        public DateTime StreamStartTime { get; set; }
        public StreamStatus StreamStatus { get; set; }
        public TimeSpan StreamDuration { get; set; }
    }
}