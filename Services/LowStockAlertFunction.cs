using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Functions;

public class LowStockAlertFunction
{
    private readonly LowStockCheckService _checkService;
    private readonly ILogger<LowStockAlertFunction> _logger;

    public LowStockAlertFunction(LowStockCheckService checkService, ILogger<LowStockAlertFunction> logger)
    {
        checkService = checkService;
        logger = logger;
    }

    public override bool Equals(object? obj)
    {
        return obj is LowStockAlertFunction function &&
               EqualityComparer<InventoryDbContext>.Default.Equals(_db, function._db) &&
               EqualityComparer<IEmailAlertService>.Default.Equals(_emailAlertService, function._emailAlertService) &&
               EqualityComparer<ILogger<LowStockAlertFunction>>.Default.Equals(this._logger, function._logger) &&
               EqualityComparer<LowStockCheckService>.Default.Equals(_checkService, function._checkService) &&
               EqualityComparer<ILogger<LowStockAlertFunction>>.Default.Equals(this._logger, function._logger);
    }

    // Note: this relies on AzureWebJobsStorage for its schedule state. Locally that's Azurite
    // (see docker-compose.yml). On Render's free tier there's no real Azure Storage behind
    // UseDevelopmentStorage=true, so this trigger may not fire reliably once deployed there.
    // Use the manual "POST /api/low-stock-check/run" endpoint (ManualLowStockCheckFunction)
    // as the reliable path for a live demo.
    [Function("LowStockAlertTimer")]
    public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo) => await _checkService.RunAsync();
}