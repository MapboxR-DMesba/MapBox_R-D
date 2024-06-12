using GoogleMap.DBModels;
using Microsoft.EntityFrameworkCore;

namespace GoogleMap.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
                
        }
      
    
        public DbSet<PolygonCenter> PolygonCenters { get; set; }
        public DbSet<PolygonPoint> PolygonPoints { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}
