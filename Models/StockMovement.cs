namespace InventoryTracker.Functions.Models;

public enum StockMovementReason
{
    Received,
    Sold,
    Adjusted,
    Wastage
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Positive = stock added (e.g. delivery received), negative = stock removed (e.g. sold).
    public int QuantityChange { get; set; }
    public StockMovementReason Reason { get; set; }
    public string? Note { get; set; }
    public string? StaffUser { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
