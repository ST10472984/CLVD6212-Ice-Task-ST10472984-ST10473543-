using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Services;

/// <summary>
/// Shared logic for checking low stock and sending an alert. Used by both the scheduled
/// Timer trigger and the manual HTTP trigger, so the two never drift out of sync.
/// </summary>
public class LowStockCheckService
{
    private readonly InventoryDbContext _db;
    private readonly IEmailAlertService _emailAlertService;
    private readonly ILogger<LowStockCheckService> _logger;

    public LowStockCheckService(InventoryDbContext db, IEmailAlertService emailAlertService, ILogger<LowStockCheckService> logger)
    {
        _db = db;
        _emailAlertService = emailAlertService;
        _logger = logger;
    }

    public async Task<List<Product>> RunAsync(CancellationToken cancellationToken = default)
    {
        var lowStockProducts = await _db.Products
            .AsNoTracking()
            .Where(p => p.CurrentStock <= p.ReorderThreshold)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Low-stock check found {Count} item(s) at or below threshold.", lowStockProducts.Count);

        if (lowStockProducts.Count > 0)
        {
            await _emailAlertService.SendLowStockAlertAsync(lowStockProducts, cancellationToken);
        }

        return lowStockProducts;
    }
}
