using Microsoft.EntityFrameworkCore;
using SmartCell.Models;

namespace SmartCell.Services.Inventory
{
    public interface IInventoryService
    {
        Task AddInventoryItemAsync(InventoryItem item);
        Task UpdateInventoryItemAsync(long id, InventoryItem updates);
        Task DeleteInventoryItemAsync(long id);
    }

    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;

        public InventoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            item.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            item.Date = DateTime.Now.ToString("MMM dd, yyyy");
            
            _context.InventoryItems.Add(item);
            AddActivity("Added", item.Name, item.Qty, item.Status);
            
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInventoryItemAsync(long id, InventoryItem updates)
        {
            var existing = await _context.InventoryItems.FindAsync(id);
            if (existing != null)
            {
                if (updates.Name != null) existing.Name = updates.Name;
                if (updates.Sku != null) existing.Sku = updates.Sku;
                if (updates.Category != null) existing.Category = updates.Category;
                existing.Qty = updates.Qty;
                existing.Price = updates.Price;
                if (updates.Supplier != null) existing.Supplier = updates.Supplier;
                if (updates.Status != null) existing.Status = updates.Status;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteInventoryItemAsync(long id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item != null)
            {
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        private void AddActivity(string action, string itemText, int qty, string status)
        {
            var entry = new ActivityLogEntry {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Action = action,
                Item = itemText,
                Qty = qty,
                Status = status,
                Time = "Just now"
            };
            
            _context.ActivityLogs.Add(entry);
            
            var count = _context.ActivityLogs.Count();
            if (count >= 10)
            {
                var oldest = _context.ActivityLogs.OrderBy(a => a.Id).First();
                _context.ActivityLogs.Remove(oldest);
            }
        }
    }
}
