using Microsoft.EntityFrameworkCore;
using SmartCell.Models;

namespace SmartCell.Services.DataStructures
{
    public interface IHashingService
    {
        Task<List<long?>> GetHashingTableAsync();
        Task UpdateHashingUnitAsync(int index, long? itemId);
        Task<HashResult> HashItemAsync(long itemId);
        Task ClearHashingTableAsync();
    }

    public class HashResult
    {
        public bool Success { get; set; }
        public int InitialIndex { get; set; }
        public int FinalIndex { get; set; }
        public bool CollisionDetected { get; set; }
        public int Steps { get; set; }
        public List<string> Log { get; set; } = new List<string>();
    }

    public class HashingService : IHashingService
    {
        private readonly AppDbContext _context;

        public HashingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<long?>> GetHashingTableAsync()
        {
            var units = await _context.HashingTable.OrderBy(h => h.Index).ToListAsync();
            return units.Select(u => u.ItemId).ToList();
        }

        public async Task UpdateHashingUnitAsync(int index, long? itemId)
        {
            var unit = await _context.HashingTable.FindAsync(index);
            if (unit != null)
            {
                unit.ItemId = itemId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearHashingTableAsync()
        {
            var units = await _context.HashingTable.ToListAsync();
            foreach (var unit in units)
            {
                unit.ItemId = null;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<HashResult> HashItemAsync(long itemId)
        {
            var result = new HashResult();
            var item = await _context.InventoryItems.FindAsync(itemId);
            
            if (item == null) return result;

            string key = item.Name;
            int asciiSum = key.Sum(c => (int)c);

            int tableSize = 13;
            int initialIndex = asciiSum % tableSize;
            int actualIndex = initialIndex;
            bool collisionDetected = false;

            result.Log.Add($"Hashing \"{key}\"...");
            result.Log.Add($"> ASCII Sum: {asciiSum}");
            result.Log.Add($"> (Sum % {tableSize}) = {initialIndex}");

            var units = await _context.HashingTable.OrderBy(h => h.Index).ToListAsync();
            int steps = 0;
            
            // Linear probing logic is now straightforward and directly queries the EF elements
            while (units[actualIndex].ItemId != null && steps < tableSize)
            {
                if (!collisionDetected)
                {
                    collisionDetected = true;
                    result.Log.Add($"! Collision at unit {actualIndex}");
                }
                actualIndex = (actualIndex + 1) % tableSize;
                steps++;
                result.Log.Add($"> Probing unit {actualIndex}...");
            }

            result.InitialIndex = initialIndex;
            result.FinalIndex = actualIndex;
            result.CollisionDetected = collisionDetected;
            result.Steps = steps;

            if (steps >= tableSize)
            {
                result.Log.Add("❌ Error: Hash table is full!");
                result.Success = false;
                return result;
            }

            units[actualIndex].ItemId = itemId;
            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.Log.Add(collisionDetected 
                ? $"✓ Stored at unit {actualIndex} after linear probing." 
                : $"✓ Stored successfully at unit {actualIndex}.");

            return result;
        }
    }
}
