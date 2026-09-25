using InventoryTracker.Functions.Models;

namespace InventoryTracker.Functions.Services;

public interface IEmailAlertService
{
    Task SendLowStockAlertAsync(IReadOnlyList<Product> lowStockProducts, CancellationToken cancellationToken = default);
}
