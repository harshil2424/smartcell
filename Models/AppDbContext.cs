using Microsoft.EntityFrameworkCore;

namespace SmartCell.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }
        public DbSet<QueueLogEntry> QueueLogs { get; set; }
        public DbSet<ActivityLogEntry> ActivityLogs { get; set; }
        public DbSet<HashingUnit> HashingTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Since IDs are manually generated in the application, avoid EF using auto-increment for them
            modelBuilder.Entity<InventoryItem>()
                .Property(i => i.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<ActivityLogEntry>()
                .Property(a => a.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<QueueLogEntry>()
                .Property(q => q.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<HashingUnit>()
                .HasKey(h => h.Index);

            modelBuilder.Entity<HashingUnit>()
                .Property(h => h.Index)
                .ValueGeneratedNever();
            
            // Initial seed data for Hashing Table (13 units)
            var initialHashingUnits = new List<HashingUnit>();
            for (int i = 0; i < 13; i++)
            {
                initialHashingUnits.Add(new HashingUnit { Index = i, ItemId = null });
            }
            modelBuilder.Entity<HashingUnit>().HasData(initialHashingUnits);
        }
    }

    public class HashingUnit
    {
        public int Index { get; set; }
        public long? ItemId { get; set; }
    }
}
