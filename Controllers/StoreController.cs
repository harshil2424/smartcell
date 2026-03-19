using Microsoft.AspNetCore.Mvc;
using SmartCell.Models;
using SmartCell.Services;

namespace SmartCell.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public async Task<ActionResult<StoreData>> Get()
        {
            return Ok(await _storeService.GetStoreDataAsync());
        }

        // --- Inventory ---
        [HttpPost("inventory")]
        public async Task<IActionResult> AddInventory([FromBody] InventoryItem item)
        {
            await _storeService.AddInventoryItemAsync(item);
            return Ok();
        }

        [HttpPut("inventory/{id}")]
        public async Task<IActionResult> UpdateInventory(long id, [FromBody] InventoryItem item)
        {
            await _storeService.UpdateInventoryItemAsync(id, item);
            return Ok();
        }

        [HttpDelete("inventory/{id}")]
        public async Task<IActionResult> DeleteInventory(long id)
        {
            await _storeService.DeleteInventoryItemAsync(id);
            return Ok();
        }

        // --- Orders ---
        [HttpPost("orders")]
        public async Task<IActionResult> AddOrder([FromBody] Order order)
        {
            await _storeService.AddOrderAsync(order);
            return Ok();
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string status)
        {
            await _storeService.UpdateOrderStatusAsync(id, status);
            return Ok();
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            await _storeService.DeleteOrderAsync(id);
            return Ok();
        }

        // --- Queue ---
        [HttpPost("queue/enqueue")]
        public async Task<IActionResult> Enqueue([FromBody] QueueItem item)
        {
            await _storeService.EnqueueAsync(item);
            return Ok();
        }

        [HttpPost("queue/process/{id}")]
        public async Task<IActionResult> Process(string id)
        {
            await _storeService.MoveToInProgressAsync(id);
            return Ok();
        }

        [HttpPost("queue/deliver/{id}")]
        public async Task<IActionResult> Deliver(string id)
        {
            await _storeService.MoveToDeliveredAsync(id);
            return Ok();
        }

        [HttpDelete("queue/log")]
        public async Task<IActionResult> ClearLog()
        {
            await _storeService.ClearQueueLogAsync();
            return Ok();
        }

        // --- Hashing ---
        [HttpGet("hashing")]
        public async Task<ActionResult<List<long?>>> GetHashingTable()
        {
            return Ok(await _storeService.GetHashingTableAsync());
        }

        [HttpPut("hashing/{index}")]
        public async Task<IActionResult> UpdateHashingUnit(int index, [FromBody] long? itemId)
        {
            await _storeService.UpdateHashingUnitAsync(index, itemId);
            return Ok();
        }
    }
}
