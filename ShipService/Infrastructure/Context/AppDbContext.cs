
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ShipService.Infrastructure.Entities;

namespace ShipService.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Ship> Ships { get; set; }
    }
}
