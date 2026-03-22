using SmartCell.Models;
using SmartCell.Services.Core;

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
        private readonly IJsonStorageService _storage;

        public HashingService(IJsonStorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<long?>> GetHashingTableAsync()
        {
            var data = await _storage.GetStoreDataAsync();
            return data.HashingTable;
        }

        public async Task UpdateHashingUnitAsync(int index, long? itemId)
        {
            var data = await _storage.GetStoreDataAsync();
            if (index >= 0 && index < data.HashingTable.Count)
            {
                data.HashingTable[index] = itemId;
                await _storage.SaveStoreDataAsync(data);
            }
        }

        public async Task ClearHashingTableAsync()
        {
            var data = await _storage.GetStoreDataAsync();
            for (int i = 0; i < data.HashingTable.Count; i++)
                data.HashingTable[i] = null;
            await _storage.SaveStoreDataAsync(data);
        }

        public async Task<HashResult> HashItemAsync(long itemId)
        {
            var data = await _storage.GetStoreDataAsync();
            var item = data.Inventory.Find(x => x.Id == itemId);
            var result = new HashResult();
            
            if (item == null) return result;

            string key = item.Name;
            int asciiSum = 0;
            foreach (char c in key) asciiSum += (int)c;

            int tableSize = 13;
            int initialIndex = asciiSum % tableSize;
            int actualIndex = initialIndex;
            bool collisionDetected = false;

            result.Log.Add($"Hashing \"{key}\"...");
            result.Log.Add($"> ASCII Sum: {asciiSum}");
            result.Log.Add($"> (Sum % {tableSize}) = {initialIndex}");

            int steps = 0;
            while (data.HashingTable[actualIndex] != null && steps < tableSize)
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

            // Success: Store it
            data.HashingTable[actualIndex] = itemId;
            await _storage.SaveStoreDataAsync(data);
            
            result.Success = true;
            if (collisionDetected)
                result.Log.Add($"✓ Stored at unit {actualIndex} after linear probing.");
            else
                result.Log.Add($"✓ Stored successfully at unit {actualIndex}.");

            return result;
        }
    }
}
