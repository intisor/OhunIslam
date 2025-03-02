using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OhunIslam.Shared.Models;


namespace OhunIslam.WebAPI.Model
{
    public class MediaItem
    {
        public int MediaId { get; set; }
        public string MediaTitle { get; set; }
        public string MediaDescription { get; set; }
        public string MediaLecturer { get; set; }
        public string MediaPath { get; set; }
        [NotMapped]
        public IFormFile MediaFile { get; set; }
        public DateTime DateIssued { get; set; }
    }

    public class ConsumedMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MessageContent { get; set; }

        public string MediaTitle { get; set; }
        public DateTime StreamStartTime { get; set; }
        public string StreamStatus { get; set; } // e.g., "Started", "Stopped"
        public TimeSpan StreamDuration { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        public static ConsumedMessage FromShared(RadioStreamingStatus Status)
        {
            return new ConsumedMessage()
            {
                MediaTitle = Status.MediaTitle,
                StreamStartTime = Status.StreamStartTime,
                StreamStatus = Status.StreamStatus,
                StreamDuration = Status.StreamDuration
            };
        }
    }
}
