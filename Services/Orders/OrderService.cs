using SmartCell.Models;
using SmartCell.Services.Core;

namespace SmartCell.Services.Orders
{
    public interface IOrderService
    {
        Task AddOrderAsync(Order order);
        Task UpdateOrderStatusAsync(string id, string status);
        Task DeleteOrderAsync(string id);
    }

    public class OrderService : IOrderService
    {
        private readonly IJsonStorageService _storage;

        public OrderService(IJsonStorageService storage)
        {
            _storage = storage;
        }

        public async Task AddOrderAsync(Order order)
        {
            var data = await _storage.GetStoreDataAsync();
            order.Id = string.IsNullOrWhiteSpace(order.Id) ? "#SC-" + new Random().Next(1000, 9999) : order.Id;
            order.Date = string.IsNullOrWhiteSpace(order.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : order.Date;
            data.Orders.Add(order);
            AddActivity(data, "Ordered", order.Item, 1, "Pending");
            await _storage.SaveStoreDataAsync(data);
        }

        public async Task UpdateOrderStatusAsync(string id, string status)
        {
            var data = await _storage.GetStoreDataAsync();
            var o = data.Orders.Find(x => x.Id == id);
            if (o != null)
            {
                o.Status = status;
                await _storage.SaveStoreDataAsync(data);
            }
        }

        public async Task DeleteOrderAsync(string id)
        {
            var data = await _storage.GetStoreDataAsync();
            data.Orders.RemoveAll(o => o.Id == id);
            data.DeliveryQueue.RemoveAll(o => o.Id == id);
            data.QueueInProgress.RemoveAll(o => o.Id == id);
            data.QueueDelivered.RemoveAll(o => o.Id == id);
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
