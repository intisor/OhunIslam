using Microsoft.AspNetCore.Mvc;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Model;
using System.Collections.Generic;
using System.Linq;

namespace OhunIslam.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaItemsController : ControllerBase
    {
        private readonly MediaContext _context;
        private static List<MediaItem> mediaItems = new List<MediaItem>();
        private string storagePath = Path.Combine(Directory.GetCurrentDirectory(), "AudioFiles");

        public MediaItemsController(MediaContext context)
        {
            _context = context;
        }
        [HttpGet]
        public ActionResult<IEnumerable<MediaItem>> Get()
        {
            return Ok(mediaItems);
        }

        [HttpGet("{id}")]
        public ActionResult<MediaItem> Get(int id)
        {
            var item = mediaItems.FirstOrDefault(m => m.MediaId == id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public ActionResult<MediaItem> Post([FromForm] MediaItemForm mediaItemForm)
        {
            var filePath = Path.Combine(storagePath, mediaItemForm.MediaFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                mediaItemForm.MediaFile.CopyTo(stream);
            }
            var mediaItem = new MediaItem
            {
                MediaTitle = mediaItemForm.MediaTitle,
                MediaDescription = mediaItemForm.MediaDescription,
                MediaLecturer = mediaItemForm.MediaLecturer,
                DateIssued = DateTime.Now,
                MediaPath = filePath,
                MediaId = mediaItems.Count + 1
            };
            _context.MediaItem.Add(mediaItem);
            _context.SaveChanges();
            return Ok(mediaItem);
            //return CreatedAtAction(nameof(Get), new { id = mediaItem.MediaId }, mediaItem);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MediaItem mediaItem)
        {
            var item = mediaItems.FirstOrDefault(m => m.MediaId == id);
            if (item == null)
            {
                return NotFound();
            }
            item.MediaTitle = mediaItem.MediaTitle;
            item.MediaDescription = mediaItem.MediaDescription;
            item.MediaLecturer = mediaItem.MediaLecturer;
            item.MediaPath = mediaItem.MediaPath;
            item.DateIssued = mediaItem.DateIssued;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = mediaItems.FirstOrDefault(m => m.MediaId == id);
            if (item == null)
            {
                return NotFound();
            }
            mediaItems.Remove(item);
            return NoContent();
        }

        [HttpGet("stream/{id}")]
        public async Task<IActionResult> StreamAudio(int id)
        {
            var item = mediaItems.FirstOrDefault(m => m.MediaId == id);
            if (item == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(storagePath, item.MediaPath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "audio/mpeg", Path.GetFileName(filePath));
        }
        public class MediaItemForm
        {
            public string MediaTitle { get; set; }
            public string MediaDescription { get; set; }
            public string MediaLecturer { get; set; }
            public IFormFile MediaFile { get; set; }
        }
    }
}
