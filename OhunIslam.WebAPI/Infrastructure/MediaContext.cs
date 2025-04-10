using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OhunIslam.Shared.Models;
using OhunIslam.WebAPI.Model;

namespace OhunIslam.WebAPI.Infrastructure
{
    public class MediaContext : DbContext
    {
        public MediaContext(DbContextOptions<MediaContext> options) : base(options)
        { }
        public DbSet<MediaItem> MediaItem { get; set; }
        public DbSet<ConsumedMessage> ConsumedMessages { get; set; } 

        public DbSet<StreamStats> StatsItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StreamStatsUpdate>()
                .HasNoKey();
            modelBuilder.Entity<ConsumedMessage>()
                .HasIndex(r => r.Id);

            modelBuilder.Entity<MediaItem>(entity =>
            {
                entity.ToTable("MediaItems");
                entity.HasKey(e => e.MediaId);
                entity.Property(e => e.MediaTitle);
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
