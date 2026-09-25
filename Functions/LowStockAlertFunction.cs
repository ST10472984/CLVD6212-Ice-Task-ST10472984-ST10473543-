using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Functions;

public class LowStockAlertFunction
{
    private readonly InventoryDbContext _db;
    private readonly IEmailAlertService _emailAlertService;
    private readonly ILogger<LowStockAlertFunction> _logger;

    public LowStockAlertFunction(InventoryDbContext db, IEmailAlertService emailAlertService, ILogger<LowStockAlertFunction> logger)
    {
        _db = db;
        _emailAlertService = emailAlertService;
        _logger = logger;
    }

    // Runs daily at 08:00 UTC. For a live demo, temporarily change this to something like
    // "0 */5 * * * *" (every 5 minutes) so you can show the alert firing without waiting a day.
    [Function("LowStockAlertTimer")]
    public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo)
    {
        var lowStockProducts = await _db.Products
            .AsNoTracking()
            .Where(p => p.CurrentStock <= p.ReorderThreshold)
            .ToListAsync();

        _logger.LogInformation("Low-stock check found {Count} item(s) at or below threshold.", lowStockProducts.Count);

        if (lowStockProducts.Count > 0)
        {
            await _emailAlertService.SendLowStockAlertAsync(lowStockProducts);
        }
    }
}
