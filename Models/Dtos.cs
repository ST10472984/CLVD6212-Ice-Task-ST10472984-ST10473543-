namespace InventoryTracker.Functions.Models;

public record ProductCreateRequest(
    string Name,
    string Sku,
    string Category,
    string Unit,
    int CurrentStock,
    int ReorderThreshold);

public record ProductUpdateRequest(
    string Name,
    string Category,
    string Unit,
    int ReorderThreshold);

public record StockMovementRequest(
    int QuantityChange,
    StockMovementReason Reason,
    string? Note,
    string? StaffUser);
