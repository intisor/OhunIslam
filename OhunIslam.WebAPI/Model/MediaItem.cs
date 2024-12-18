using System.ComponentModel.DataAnnotations.Schema;

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
}
