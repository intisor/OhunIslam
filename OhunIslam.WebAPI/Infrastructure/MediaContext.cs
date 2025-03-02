using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OhunIslam.WebAPI.Model;

namespace OhunIslam.WebAPI.Infrastructure
{
    public class MediaContext : DbContext
    {
        public MediaContext(DbContextOptions<MediaContext> options) : base(options)
        { }
        public DbSet<MediaItem> MediaItem { get; set; }
        public DbSet<ConsumedMessage> ConsumedMessages { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
             modelBuilder.Entity<ConsumedMessage>()
                .HasIndex(r => r.StreamStartTime);
                
            modelBuilder.Entity<ConsumedMessage>()
                .HasIndex(r => r.StreamStatus);

            modelBuilder.Entity<MediaItem>(entity =>
            {
                entity.ToTable("MediaItems");
                entity.HasKey(e => e.MediaId);
                entity.Property(e => e.MediaTitle).IsRequired();
                entity.Property(e => e.MediaLecturer);
                entity.Property(e => e.MediaPath);
                entity.Property(e => e.MediaDescription);
                entity.Property(e => e.DateIssued);
            });
        }

    }

    public class MediaContextFactory : IDesignTimeDbContextFactory<MediaContext>
    {
        public MediaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MediaContext>();
            optionsBuilder.UseSqlServer("Server=INTITECH;Database=OhunIslam;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;");

            return new MediaContext(optionsBuilder.Options);
        }
    }

}
