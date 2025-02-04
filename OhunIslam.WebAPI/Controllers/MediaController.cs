using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OhunIslam.WebAPI.Infrastructure;
using OhunIslam.WebAPI.Model;
using OhunIslam.WebAPI.Services;
using System.Collections.Generic;
using System.Linq;

namespace OhunIslam.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaItemsController : ControllerBase
    {
        private readonly MediaContext _context;
        private readonly WebRabbitMQService _webRabbitMQService;
        private string storagePath = Path.Combine(Directory.GetCurrentDirectory(), "AudioFiles");

        public MediaItemsController(MediaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaItem>>> Get()
        {
            return Ok(await _context.MediaItem.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MediaItem>> Get(int id)
        {
            var item = await _context.MediaItem.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            _webRabbitMQService.PublishToRadio($"Media item retrieved at :-  {DateTime.Now}");
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<MediaItem>> Post([FromForm] MediaItemForm mediaItemForm)
        {
            var filePath = Path.Combine(storagePath, mediaItemForm.MediaFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await mediaItemForm.MediaFile.CopyToAsync(stream);
            }
            var mediaItem = new MediaItem
            {
                MediaTitle = mediaItemForm.MediaTitle,
                MediaDescription = mediaItemForm.MediaDescription,
                MediaLecturer = mediaItemForm.MediaLecturer,
                DateIssued = DateTime.Now,
                MediaPath = filePath
            };
            _context.MediaItem.Add(mediaItem);
            await _context.SaveChangesAsync();
            _webRabbitMQService.PublishToRadio($"Media item created at :-  {DateTime.Now}");
            return CreatedAtAction(nameof(Get), new { id = mediaItem.MediaId }, mediaItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] MediaItem mediaItem)
        {
            if (id != mediaItem.MediaId)
            {
                return BadRequest();
            }

            _context.Entry(mediaItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MediaItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.MediaItem.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.MediaItem.Remove(item);
            await _context.SaveChangesAsync();
            _webRabbitMQService.PublishToRadio($"Media item deleted at :-  {DateTime.Now}");
            return NoContent();
        }

        [HttpGet("stream/{id}")]
        public async Task<IActionResult> StreamAudio(int id)
        {
            var item = await _context.MediaItem.FindAsync(id);
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
            _webRabbitMQService.PublishToRadio($"Media item streamed at :-  {DateTime.Now}");
            return File(memory, "audio/mpeg", Path.GetFileName(filePath));
        }

        private bool MediaItemExists(int id)
        {
            return _context.MediaItem.Any(e => e.MediaId == id);
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
