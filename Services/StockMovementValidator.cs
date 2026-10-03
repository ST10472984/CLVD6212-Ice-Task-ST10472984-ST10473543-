namespace InventoryTracker.Functions.Services;

/// <summary>
/// Pure validation logic for stock movements, kept separate from the HTTP-triggered
/// function so it can be unit tested without spinning up the Functions host.
/// </summary>
public static class StockMovementValidator
{
    public static bool WouldResultInNegativeStock(int currentStock, int quantityChange)
    {
        return currentStock + quantityChange < 0;
    }

    public static int CalculateNewStock(int currentStock, int quantityChange)
    {
        return currentStock + quantityChange;
    }
}
