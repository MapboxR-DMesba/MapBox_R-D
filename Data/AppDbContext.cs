using GoogleMap.DBModels;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GoogleMap.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
         
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolygonCenter>()
                .HasMany(p => p.PolygonPoints)
                .WithOne(c => c.PolygonCenter)
                .HasForeignKey(c => c.CenterId);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<PolygonCenter> PolygonCenters { get; set; }
        public DbSet<PolygonPoint> PolygonPoints { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}
