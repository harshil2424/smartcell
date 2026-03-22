using SmartCell.Models;
using SmartCell.Services.Core;

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
        private readonly IJsonStorageService _storage;

        public InventoryService(IJsonStorageService storage)
        {
            _storage = storage;
        }

        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            var data = await _storage.GetStoreDataAsync();
            item.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            item.Date = DateTime.Now.ToString("MMM dd, yyyy");
            data.Inventory.Add(item);
            AddActivity(data, "Added", item.Name, item.Qty, item.Status);
            await _storage.SaveStoreDataAsync(data);
        }

        public async Task UpdateInventoryItemAsync(long id, InventoryItem updates)
        {
            var data = await _storage.GetStoreDataAsync();
            var existing = data.Inventory.Find(i => i.Id == id);
            if (existing != null)
            {
                if (updates.Name != null) existing.Name = updates.Name;
                if (updates.Sku != null) existing.Sku = updates.Sku;
                if (updates.Category != null) existing.Category = updates.Category;
                existing.Qty = updates.Qty;
                existing.Price = updates.Price;
                if (updates.Supplier != null) existing.Supplier = updates.Supplier;
                if (updates.Status != null) existing.Status = updates.Status;
                await _storage.SaveStoreDataAsync(data);
            }
        }

        public async Task DeleteInventoryItemAsync(long id)
        {
            var data = await _storage.GetStoreDataAsync();
            data.Inventory.RemoveAll(i => i.Id == id);
            await _storage.SaveStoreDataAsync(data);
        }

        private void AddActivity(StoreData data, string action, string item, int qty, string status)
        {
            data.RecentActivity.Insert(0, new ActivityLogEntry {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Action = action,
                Item = item,
                Qty = qty,
                Status = status,
                Time = "Just now"
            });
            if (data.RecentActivity.Count > 10) data.RecentActivity.RemoveAt(10);
        }
    }
}
