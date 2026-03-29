using Microsoft.EntityFrameworkCore;
using SmartCell.Models;

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
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(Order order)
        {
            order.Id = string.IsNullOrWhiteSpace(order.Id) ? "#SC-" + new Random().Next(1000, 9999) : order.Id;
            order.Date = string.IsNullOrWhiteSpace(order.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : order.Date;
            
            _context.Orders.Add(order);
            AddActivity("Ordered", order.Item, 1, "Pending");
            
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderStatusAsync(string id, string status)
        {
            var o = await _context.Orders.FindAsync(id);
            if (o != null)
            {
                o.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOrderAsync(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null) _context.Orders.Remove(order);

            var queueItems = await _context.QueueItems.Where(q => q.Id == id).ToListAsync();
            _context.QueueItems.RemoveRange(queueItems);
            
            await _context.SaveChangesAsync();
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
