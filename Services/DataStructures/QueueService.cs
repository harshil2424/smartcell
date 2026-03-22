using SmartCell.Models;
using SmartCell.Services.Core;

namespace SmartCell.Services.DataStructures
{
    public interface IQueueService
    {
        Task EnqueueAsync(QueueItem item);
        Task<QueueItem?> DequeueAsync();
        Task MoveToInProgressAsync(string id);
        Task MoveToDeliveredAsync(string id);
        Task ClearQueueLogAsync();
    }

    public class QueueService : IQueueService
    {
        private readonly IJsonStorageService _storage;

        public QueueService(IJsonStorageService storage)
        {
            _storage = storage;
        }

        public async Task EnqueueAsync(QueueItem item)
        {
            var data = await _storage.GetStoreDataAsync();
            await EnqueueInternalAsync(data, item);
            await _storage.SaveStoreDataAsync(data);
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

        public async Task<QueueItem?> DequeueAsync()
        {
            var data = await _storage.GetStoreDataAsync();
            if (data.DeliveryQueue.Count == 0) return null;

            // Strict FIFO: pop front index 0
            var o = data.DeliveryQueue[0];
            data.DeliveryQueue.RemoveAt(0);
            data.QueueInProgress.Add(o);
            
            AddQueueLog(data, "PROCESS", o.Id, o.Item, "In Progress");
            var order = data.Orders.Find(x => x.Id == o.Id);
            if (order != null) order.Status = "processing";
            
            await _storage.SaveStoreDataAsync(data);
            return o;
        }

        public async Task MoveToInProgressAsync(string id)
        {
            var data = await _storage.GetStoreDataAsync();
            var o = data.DeliveryQueue.Find(x => x.Id == id);
            if (o != null)
            {
                data.DeliveryQueue.Remove(o);
                data.QueueInProgress.Add(o);
                AddQueueLog(data, "PROCESS", id, o.Item, "In Progress");
                
                var order = data.Orders.Find(x => x.Id == id);
                if (order != null) order.Status = "processing";
                
                await _storage.SaveStoreDataAsync(data);
            }
        }

        public async Task MoveToDeliveredAsync(string id)
        {
            var data = await _storage.GetStoreDataAsync();
            var o = data.QueueInProgress.Find(x => x.Id == id);
            if (o != null)
            {
                data.QueueInProgress.Remove(o);
                data.QueueDelivered.Add(o);
                AddQueueLog(data, "DELIVER", id, o.Item, "Delivered");
                
                var order = data.Orders.Find(x => x.Id == id);
                if (order != null) order.Status = "delivered";
                
                AddActivity(data, "Delivered", o.Item, o.Qty, "Completed");
                await _storage.SaveStoreDataAsync(data);
            }
        }

        public async Task ClearQueueLogAsync()
        {
            var data = await _storage.GetStoreDataAsync();
            data.QueueLog.Clear();
            await _storage.SaveStoreDataAsync(data);
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
