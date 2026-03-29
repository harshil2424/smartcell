using System.Text.Json.Serialization;

namespace SmartCell.Models
{
    public class StoreData
    {
        [JsonPropertyName("inventory")]
        public List<InventoryItem> Inventory { get; set; } = new();

        [JsonPropertyName("orders")]
        public List<Order> Orders { get; set; } = new();

        [JsonPropertyName("deliveryQueue")]
        public List<QueueItem> DeliveryQueue { get; set; } = new();

        [JsonPropertyName("queueInProgress")]
        public List<QueueItem> QueueInProgress { get; set; } = new();

        [JsonPropertyName("queueDelivered")]
        public List<QueueItem> QueueDelivered { get; set; } = new();

        [JsonPropertyName("queueLog")]
        public List<QueueLogEntry> QueueLog { get; set; } = new();

        [JsonPropertyName("recentActivity")]
        public List<ActivityLogEntry> RecentActivity { get; set; } = new();

        [JsonPropertyName("hashingTable")]
        public List<long?> HashingTable { get; set; } = new();
    }

    public class InventoryItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("qty")]
        public int Qty { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("supplier")]
        public string Supplier { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "In Stock";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";
    }

    public class Order
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("customer")]
        public string Customer { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";
    }

    public class QueueItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("customer")]
        public string Customer { get; set; } = "";

        [JsonPropertyName("address")]
        public string Address { get; set; } = "";

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "Medium";

        [JsonPropertyName("qty")]
        public int Qty { get; set; } = 1;

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonIgnore]
        public string QueueType { get; set; } = "DeliveryQueue";
    }

    public class QueueLogEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; } = "";

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = "";

        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";
    }

    public class ActivityLogEntry
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = "";

        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("qty")]
        public int Qty { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("time")]
        public string Time { get; set; } = "Just now";
    }
}
