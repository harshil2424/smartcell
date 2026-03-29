using Microsoft.EntityFrameworkCore;
using SmartCell.Models;

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
        private readonly AppDbContext _context;

        public QueueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task EnqueueAsync(QueueItem item)
        {
            if (await _context.QueueItems.AnyAsync(q => q.Id == item.Id))
                return;

            item.Id = item.Id ?? "#SC-" + new Random().Next(1000, 9999);
            item.Time = DateTime.Now.ToString("hh:mm tt");
            item.Date = DateTime.Now.ToString("MMM dd");
            item.QueueType = "DeliveryQueue";

            _context.QueueItems.Add(item);
            
            LogEvent("ENQUEUE", item.Id, item.Item, "Pending");

            if (!await _context.Orders.AnyAsync(o => o.Id == item.Id))
            {
                _context.Orders.Add(new Order {
                    Id = item.Id,
                    Customer = item.Customer,
                    Item = item.Item,
                    Status = "pending",
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Email = item.Customer.ToLower().Replace(" ",".") + "@example.com"
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<QueueItem?> DequeueAsync()
        {
            // Strict FIFO from DeliveryQueue
            var item = await _context.QueueItems
                .Where(q => q.QueueType == "DeliveryQueue")
                .FirstOrDefaultAsync();

            if (item == null) return null;

            item.QueueType = "InProgress";
            UpdateOrderStatus(item.Id, "processing");
            LogEvent("PROCESS", item.Id, item.Item, "In Progress");

            await _context.SaveChangesAsync();
            return item;
        }

        public async Task MoveToInProgressAsync(string id)
        {
            var item = await _context.QueueItems.FindAsync(id);
            if (item != null && item.QueueType == "DeliveryQueue")
            {
                item.QueueType = "InProgress";
                UpdateOrderStatus(id, "processing");
                LogEvent("PROCESS", id, item.Item, "In Progress");
                await _context.SaveChangesAsync();
            }
        }

        public async Task MoveToDeliveredAsync(string id)
        {
            var item = await _context.QueueItems.FindAsync(id);
            if (item != null && item.QueueType == "InProgress")
            {
                item.QueueType = "Delivered";
                UpdateOrderStatus(id, "delivered");
                LogEvent("DELIVER", id, item.Item, "Delivered");
                AddActivity("Delivered", item.Item, item.Qty, "Completed");
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearQueueLogAsync()
        {
            _context.QueueLogs.RemoveRange(_context.QueueLogs);
            await _context.SaveChangesAsync();
        }

        private void LogEvent(string ev, string orderId, string itemText, string status)
        {
            int nextId = (_context.QueueLogs.Max(q => (int?)q.Id) ?? 0) + 1;
            var log = new QueueLogEntry {
                Id = nextId,
                Event = ev,
                OrderId = orderId,
                Item = itemText,
                Status = status,
                Time = DateTime.Now.ToString("hh:mm tt")
            };
            
            _context.QueueLogs.Add(log);
            
            // Delete old logs if more than 50
            var count = _context.QueueLogs.Count();
            if (count >= 50)
            {
                var oldest = _context.QueueLogs.OrderBy(q => q.Id).First();
                _context.QueueLogs.Remove(oldest);
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

        private void UpdateOrderStatus(string id, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order != null) order.Status = status;
        }
    }
}
