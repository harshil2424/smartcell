using Microsoft.AspNetCore.Mvc;
using SmartCell.Models;
using SmartCell.Services.Inventory;
using SmartCell.Services.Orders;
using SmartCell.Services.Core;

namespace SmartCell.Controllers
{
    [ApiController]
    [Route("api/Store")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly IJsonStorageService _storageService;

        public InventoryController(IInventoryService inventoryService, IOrderService orderService, IJsonStorageService storageService)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<ActionResult<StoreData>> Get()
        {
            return Ok(await _storageService.GetStoreDataAsync());
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
