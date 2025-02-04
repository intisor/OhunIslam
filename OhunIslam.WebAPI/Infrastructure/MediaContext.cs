using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OhunIslam.WebAPI.Model;

namespace OhunIslam.WebAPI.Infrastructure
{
    public class MediaContext : DbContext
    {
        public MediaContext(DbContextOptions<MediaContext> options) : base(options)
        { }
        public DbSet<MediaItem> MediaItem { get; set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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
            optionsBuilder.UseSqlServer("Server=localhost;Database=OhunIslam;User Id=INTITECH;password=Mawupego777#;TrustServerCertificate=True;");

            return new MediaContext(optionsBuilder.Options);
        }
    }

}
