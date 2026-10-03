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


    // Note: this relies on AzureWebJobsStorage for its schedule state. Locally that's Azurite
    // (see docker-compose.yml). On Render's free tier there's no real Azure Storage behind
    // UseDevelopmentStorage=true, so this trigger may not fire reliably once deployed there.
    // Use the manual "POST /api/low-stock-check/run" endpoint (ManualLowStockCheckFunction)
    // as the reliable path for a live demo.
    [Function("LowStockAlertTimer")]
    public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo) => await _checkService.RunAsync();
}