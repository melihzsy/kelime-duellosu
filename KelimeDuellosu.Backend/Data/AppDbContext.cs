using Microsoft.EntityFrameworkCore;
using KelimeDuellosu.Backend.Models; // Word sınıfının olduğu yer

namespace KelimeDuellosu.Backend.Data
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Word> Words { get; set; }
    }
}