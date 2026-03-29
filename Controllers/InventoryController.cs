using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCell.Models;
using SmartCell.Services.Inventory;
using SmartCell.Services.Orders;

namespace SmartCell.Controllers
{
    [ApiController]
    [Route("api/Store")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;

        public InventoryController(IInventoryService inventoryService, IOrderService orderService, AppDbContext context)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<StoreData>> Get()
        {
            // Build the StoreData state from the database
            var queueItems = await _context.QueueItems.ToListAsync();
            
            var storeData = new StoreData
            {
                Inventory = await _context.InventoryItems.ToListAsync(),
                Orders = await _context.Orders.ToListAsync(),
                DeliveryQueue = queueItems.Where(q => q.QueueType == "DeliveryQueue").ToList(),
                QueueInProgress = queueItems.Where(q => q.QueueType == "InProgress").ToList(),
                QueueDelivered = queueItems.Where(q => q.QueueType == "Delivered").ToList(),
                QueueLog = await _context.QueueLogs.OrderByDescending(q => q.Id).ToListAsync(),
                RecentActivity = await _context.ActivityLogs.OrderByDescending(a => a.Id).ToListAsync(),
                HashingTable = (await _context.HashingTable.OrderBy(h => h.Index).ToListAsync()).Select(h => h.ItemId).ToList()
            };

            return Ok(storeData);
        }

        // --- Inventory ---
        [HttpPost("inventory")]
        public async Task<IActionResult> AddInventory([FromBody] InventoryItem item)
        {
            await _inventoryService.AddInventoryItemAsync(item);
            return Ok();
        }

        [HttpPut("inventory/{id}")]
        public async Task<IActionResult> UpdateInventory(long id, [FromBody] InventoryItem item)
        {
            await _inventoryService.UpdateInventoryItemAsync(id, item);
            return Ok();
        }

        [HttpDelete("inventory/{id}")]
        public async Task<IActionResult> DeleteInventory(long id)
        {
            await _inventoryService.DeleteInventoryItemAsync(id);
            return Ok();
        }

        // --- Orders ---
        [HttpPost("orders")]
        public async Task<IActionResult> AddOrder([FromBody] Order order)
        {
            await _orderService.AddOrderAsync(order);
            return Ok();
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            return Ok();
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            await _orderService.DeleteOrderAsync(id);
            return Ok();
        }
    }
}
