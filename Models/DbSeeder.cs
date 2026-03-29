using SmartCell.Models;
using System;
using System.Linq;

namespace SmartCell.Models
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();
            
            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.AddRange(
                    new InventoryItem { Id = 1, Name = "MacBook Air M3", Sku = "ELEC-001", Category = "Electronics", Qty = 50, Price = 112000, Supplier = "Apple India", Status = "In Stock", Date = "Jan 11, 2026" },
                    new InventoryItem { Id = 2, Name = "Samsung 4K OLED TV", Sku = "ELEC-002", Category = "Electronics", Qty = 18, Price = 85000, Supplier = "Samsung Wholesale", Status = "In Stock", Date = "Jan 16, 2026" },
                    new InventoryItem { Id = 3, Name = "Sony WH-1000XM5", Sku = "ELEC-003", Category = "Electronics", Qty = 0, Price = 28000, Supplier = "Sony Dist.", Status = "Out of Stock", Date = "Feb 2, 2026" }
                );
            }

            if (!context.Orders.Any())
            {
                context.Orders.AddRange(
                    new Order { Id = "#SC-8422", Customer = "Rahul Sharma", Email = "rahul@example.com", Item = "iPhone 15 Pro", Category = "Smartphones", Date = "2026-03-18", Amount = 124900, Status = "pending" },
                    new Order { Id = "#SC-8421", Customer = "Priya Patel", Email = "priya@example.com", Item = "MacBook Air M2", Category = "Laptops", Date = "2026-03-18", Amount = 94900, Status = "processing" }
                );
            }

            if (!context.QueueItems.Any())
            {
                context.QueueItems.AddRange(
                    new QueueItem { Id = "ORD-1001", Item = "Samsung 4K TV × 1", Customer = "Rahul Sharma", Address = "Andheri, Mumbai 400053", Priority = "High", Qty = 1, Time = "10:00 AM", Date = "Mar 18", QueueType = "DeliveryQueue" },
                    new QueueItem { Id = "ORD-1002", Item = "Nike Air Max 90 × 2", Customer = "Priya Mehta", Address = "Koramangala, Bengaluru 560034", Priority = "Medium", Qty = 2, Time = "10:15 AM", Date = "Mar 18", QueueType = "DeliveryQueue" }
                );
            }

            if (!context.ActivityLogs.Any())
            {
                context.ActivityLogs.AddRange(
                    new ActivityLogEntry { Id = 1, Action = "Added", Item = "MacBook Air M3", Qty = 50, Status = "In Stock", Time = "2 min ago" },
                    new ActivityLogEntry { Id = 2, Action = "Dispatched", Item = "Samsung 4K TV", Qty = 15, Status = "Shipped", Time = "8 min ago" }
                );
            }

            context.SaveChanges();
        }
    }
}
