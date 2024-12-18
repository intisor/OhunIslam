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
            optionsBuilder.UseMySql("server=127.0.0.1;port=1234;database=OhunIslam;user=root;password=Mawupego777#",
                 new MySqlServerVersion(new Version(8, 0, 1)));

            return new MediaContext(optionsBuilder.Options);
        }
    }

}
