using Microsoft.AspNetCore.Mvc;
using SmartCell.Models;
using SmartCell.Services.DataStructures;

namespace SmartCell.Controllers
{
    [ApiController]
    [Route("api/Store")]
    public class DataStructuresController : ControllerBase
    {
        private readonly IHashingService _hashingService;
        private readonly IQueueService _queueService;

        public DataStructuresController(IHashingService hashingService, IQueueService queueService)
        {
            _hashingService = hashingService;
            _queueService = queueService;
        }

        // --- Queue ---
        [HttpPost("queue/enqueue")]
        public async Task<IActionResult> Enqueue([FromBody] QueueItem item)
        {
            await _queueService.EnqueueAsync(item);
            return Ok();
        }

        [HttpPost("queue/dequeue")]
        public async Task<IActionResult> Dequeue()
        {
            var item = await _queueService.DequeueAsync();
            if (item == null) return BadRequest("Queue is empty.");
            return Ok(item);
        }

        [HttpPost("queue/process/{id}")]
        public async Task<IActionResult> Process(string id)
        {
            await _queueService.MoveToInProgressAsync(id);
            return Ok();
        }

        [HttpPost("queue/deliver/{id}")]
        public async Task<IActionResult> Deliver(string id)
        {
            await _queueService.MoveToDeliveredAsync(id);
            return Ok();
        }

        [HttpDelete("queue/log")]
        public async Task<IActionResult> ClearLog()
        {
            await _queueService.ClearQueueLogAsync();
            return Ok();
        }

        // --- Hashing ---
        [HttpGet("hashing")]
        public async Task<ActionResult<List<long?>>> GetHashingTable()
        {
            return Ok(await _hashingService.GetHashingTableAsync());
        }

        [HttpPut("hashing/{index}")]
        public async Task<IActionResult> UpdateHashingUnit(int index, [FromBody] long? itemId)
        {
            await _hashingService.UpdateHashingUnitAsync(index, itemId);
            return Ok();
        }

        [HttpDelete("hashing")]
        public async Task<IActionResult> ClearHashingTable()
        {
            await _hashingService.ClearHashingTableAsync();
            return Ok();
        }

        [HttpPost("hashing/calculate")]
        public async Task<ActionResult<HashResult>> CalculateHash([FromBody] long itemId)
        {
            var result = await _hashingService.HashItemAsync(itemId);
            return Ok(result);
        }
    }
}
