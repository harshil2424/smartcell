using System.Text.Json;
using SmartCell.Models;

namespace SmartCell.Services.Core
{
    public interface IJsonStorageService
    {
        Task<StoreData> GetStoreDataAsync();
        Task SaveStoreDataAsync(StoreData data);
    }

    public class JsonStorageService : IJsonStorageService
    {
        private readonly string _filePath;
        private StoreData? _cache;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonStorageService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "data.json");
        }

        private async Task<StoreData> LoadAsync()
        {
            if (_cache != null) return _cache;

            if (!File.Exists(_filePath))
            {
                _cache = GetDefaultData();
                await SaveAsync(_cache);
                return _cache;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                _cache = JsonSerializer.Deserialize<StoreData>(json) ?? GetDefaultData();
                
                if (_cache.HashingTable == null || _cache.HashingTable.Count == 0)
                {
                    _cache.HashingTable = Enumerable.Repeat<long?>(null, 13).ToList();
                }
            }
            catch
            {
                _cache = GetDefaultData();
            }

            return _cache;
        }

        private async Task SaveAsync(StoreData data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
            _cache = data;
        }

        public async Task<StoreData> GetStoreDataAsync()
        {
            await _lock.WaitAsync();
            try { return await LoadAsync(); }
            finally { _lock.Release(); }
        }

        public async Task SaveStoreDataAsync(StoreData data)
        {
            await _lock.WaitAsync();
            try { await SaveAsync(data); }
            finally { _lock.Release(); }
        }

        private StoreData GetDefaultData()
        {
            return new StoreData
            {
                Inventory = new List<InventoryItem> {
                    new() { Id = 1, Name = "MacBook Air M3", Sku = "ELEC-001", Category = "Electronics", Qty = 50, Price = 112000, Supplier = "Apple India", Status = "In Stock", Date = "Jan 11, 2026" },
                    new() { Id = 2, Name = "Samsung 4K OLED TV", Sku = "ELEC-002", Category = "Electronics", Qty = 18, Price = 85000, Supplier = "Samsung Wholesale", Status = "In Stock", Date = "Jan 16, 2026" },
                    new() { Id = 3, Name = "Sony WH-1000XM5", Sku = "ELEC-003", Category = "Electronics", Qty = 0, Price = 28000, Supplier = "Sony Dist.", Status = "Out of Stock", Date = "Feb 2, 2026" }
                },
                Orders = new List<Order> {
                    new() { Id = "#SC-8422", Customer = "Rahul Sharma", Email = "rahul@example.com", Item = "iPhone 15 Pro", Category = "Smartphones", Date = "2026-03-18", Amount = 124900, Status = "pending" },
                    new() { Id = "#SC-8421", Customer = "Priya Patel", Email = "priya@example.com", Item = "MacBook Air M2", Category = "Laptops", Date = "2026-03-18", Amount = 94900, Status = "processing" }
                },
                DeliveryQueue = new List<QueueItem> {
                    new() { Id = "ORD-1001", Item = "Samsung 4K TV × 1", Customer = "Rahul Sharma", Address = "Andheri, Mumbai 400053", Priority = "High", Qty = 1, Time = "10:00 AM", Date = "Mar 18" },
                    new() { Id = "ORD-1002", Item = "Nike Air Max 90 × 2", Customer = "Priya Mehta", Address = "Koramangala, Bengaluru 560034", Priority = "Medium", Qty = 2, Time = "10:15 AM", Date = "Mar 18" }
                },
                RecentActivity = new List<ActivityLogEntry> {
                    new() { Id = 1, Action = "Added", Item = "MacBook Air M3", Qty = 50, Status = "In Stock", Time = "2 min ago" },
                    new() { Id = 2, Action = "Dispatched", Item = "Samsung 4K TV", Qty = 15, Status = "Shipped", Time = "8 min ago" }
                },
                HashingTable = Enumerable.Repeat<long?>(null, 13).ToList()
            };
        }
    }
}
