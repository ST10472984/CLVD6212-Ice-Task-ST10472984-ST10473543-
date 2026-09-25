namespace InventoryTracker.Functions.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = "each";
    public int CurrentStock { get; set; }
    public int ReorderThreshold { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<StockMovement> Movements { get; set; } = new();
}
