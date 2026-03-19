using System.Text.Json;
using SmartCell.Models;

namespace SmartCell.Services
{
    public interface IStoreService
    {
        Task<StoreData> GetStoreDataAsync();
        Task SaveStoreDataAsync(StoreData data);
        
        // Convenience methods
        Task AddInventoryItemAsync(InventoryItem item);
        Task UpdateInventoryItemAsync(long id, InventoryItem updates);
        Task DeleteInventoryItemAsync(long id);
        
        Task AddOrderAsync(Order order);
        Task UpdateOrderStatusAsync(string id, string status);
        Task DeleteOrderAsync(string id);
        
        Task EnqueueAsync(QueueItem item);
        Task MoveToInProgressAsync(string id);
        Task MoveToDeliveredAsync(string id);
        Task ClearQueueLogAsync();

        Task<List<long?>> GetHashingTableAsync();
        Task UpdateHashingUnitAsync(int index, long? itemId);
    }

    public class StoreService : IStoreService
    {
        private readonly string _filePath;
        private StoreData? _cache;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public StoreService(IWebHostEnvironment env)
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
                
                // Ensure HashingTable is initialized and correct size
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

        // --- Inventory ---
        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            var data = await GetStoreDataAsync();
            item.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            item.Date = DateTime.Now.ToString("MMM dd, yyyy");
            data.Inventory.Add(item);
            AddActivity(data, "Added", item.Name, item.Qty, item.Status);
            await SaveStoreDataAsync(data);
        }

        public async Task UpdateInventoryItemAsync(long id, InventoryItem updates)
        {
            var data = await GetStoreDataAsync();
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
                await SaveStoreDataAsync(data);
            }
        }

        public async Task DeleteInventoryItemAsync(long id)
        {
            var data = await GetStoreDataAsync();
            data.Inventory.RemoveAll(i => i.Id == id);
            await SaveStoreDataAsync(data);
        }

        // --- Orders ---
        public async Task AddOrderAsync(Order order)
        {
            var data = await GetStoreDataAsync();
            order.Id = string.IsNullOrWhiteSpace(order.Id) ? "#SC-" + new Random().Next(1000, 9999) : order.Id;
            order.Date = string.IsNullOrWhiteSpace(order.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : order.Date;
            data.Orders.Add(order);
            AddActivity(data, "Ordered", order.Item, 1, "Pending");
            await SaveStoreDataAsync(data);
        }

        public async Task UpdateOrderStatusAsync(string id, string status)
        {
            var data = await GetStoreDataAsync();
            var o = data.Orders.Find(x => x.Id == id);
            if (o != null)
            {
                o.Status = status;
                await SaveStoreDataAsync(data);
            }
        }

        public async Task DeleteOrderAsync(string id)
        {
            var data = await GetStoreDataAsync();
            data.Orders.RemoveAll(o => o.Id == id);
            data.DeliveryQueue.RemoveAll(o => o.Id == id);
            data.QueueInProgress.RemoveAll(o => o.Id == id);
            data.QueueDelivered.RemoveAll(o => o.Id == id);
            await SaveStoreDataAsync(data);
        }

        // --- Queue ---
        public async Task EnqueueAsync(QueueItem item)
        {
            var data = await GetStoreDataAsync();
            await EnqueueInternalAsync(data, item);
            await SaveStoreDataAsync(data);
        }

        private async Task EnqueueInternalAsync(StoreData data, QueueItem item)
        {
            if (data.DeliveryQueue.Any(o => o.Id == item.Id) || 
                data.QueueInProgress.Any(o => o.Id == item.Id) || 
                data.QueueDelivered.Any(o => o.Id == item.Id))
                return;

            item.Id = item.Id ?? "#SC-" + new Random().Next(1000, 9999);
            item.Time = DateTime.Now.ToString("hh:mm tt");
            item.Date = DateTime.Now.ToString("MMM dd");
            
            data.DeliveryQueue.Add(item);
            AddQueueLog(data, "ENQUEUE", item.Id, item.Item, "Pending");

            var existingOrder = data.Orders.Find(o => o.Id == item.Id);
            if (existingOrder == null)
            {
                data.Orders.Add(new Order {
                    Id = item.Id,
                    Customer = item.Customer,
                    Item = item.Item,
                    Status = "pending",
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Email = item.Customer.ToLower().Replace(" ",".") + "@example.com"
                });
            }
        }

        public async Task MoveToInProgressAsync(string id)
        {
            var data = await GetStoreDataAsync();
            var o = data.DeliveryQueue.Find(x => x.Id == id);
            if (o != null)
            {
                data.DeliveryQueue.Remove(o);
                data.QueueInProgress.Add(o);
                AddQueueLog(data, "PROCESS", id, o.Item, "In Progress");
                
                var order = data.Orders.Find(x => x.Id == id);
                if (order != null) order.Status = "processing";
                
                await SaveStoreDataAsync(data);
            }
        }

        public async Task MoveToDeliveredAsync(string id)
        {
            var data = await GetStoreDataAsync();
            var o = data.QueueInProgress.Find(x => x.Id == id);
            if (o != null)
            {
                data.QueueInProgress.Remove(o);
                data.QueueDelivered.Add(o);
                AddQueueLog(data, "DELIVER", id, o.Item, "Delivered");
                
                var order = data.Orders.Find(x => x.Id == id);
                if (order != null) order.Status = "delivered";
                
                AddActivity(data, "Delivered", o.Item, o.Qty, "Completed");
                await SaveStoreDataAsync(data);
            }
        }

        public async Task ClearQueueLogAsync()
        {
            var data = await GetStoreDataAsync();
            data.QueueLog.Clear();
            await SaveStoreDataAsync(data);
        }

        public async Task<List<long?>> GetHashingTableAsync()
        {
            var data = await GetStoreDataAsync();
            return data.HashingTable;
        }

        public async Task UpdateHashingUnitAsync(int index, long? itemId)
        {
            var data = await GetStoreDataAsync();
            if (index >= 0 && index < data.HashingTable.Count)
            {
                data.HashingTable[index] = itemId;
                await SaveStoreDataAsync(data);
            }
        }

        // --- Helpers ---
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

        private void AddQueueLog(StoreData data, string ev, string orderId, string item, string status)
        {
            data.QueueLog.Insert(0, new QueueLogEntry {
                Id = data.QueueLog.Count + 1,
                Event = ev,
                OrderId = orderId,
                Item = item,
                Status = status,
                Time = DateTime.Now.ToString("hh:mm tt")
            });
            if (data.QueueLog.Count > 50) data.QueueLog.RemoveAt(50);
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
