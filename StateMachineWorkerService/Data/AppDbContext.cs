using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineWorkerService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<OrderStateInstance> OrderStateInstances { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStateInstance>()
         .ToTable("OrderStateInstance"); // Specify the exact table name

            modelBuilder.Entity<OrderStateInstance>()
                .HasKey(o => o.CorrelationId);
        }


    }
}
