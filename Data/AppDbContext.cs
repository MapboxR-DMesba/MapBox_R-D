using GoogleMap.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleMap.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
                
        }
      
        public DbSet<Location> Location { get; set; }
    }
}
