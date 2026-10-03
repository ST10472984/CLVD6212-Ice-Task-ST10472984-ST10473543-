using System.Net;
using InventoryTracker.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Functions;

/// <summary>
/// On-demand version of the low-stock check, for demos and for environments (like Render's
/// free tier) where the Timer trigger's storage dependency isn't reliable.
/// </summary>
public class ManualLowStockCheckFunction
{
    private readonly LowStockCheckService _checkService;
    private readonly ILogger<ManualLowStockCheckFunction> _logger;

    public ManualLowStockCheckFunction(LowStockCheckService checkService, ILogger<ManualLowStockCheckFunction> logger)
    {
        _checkService = checkService;
        _logger = logger;
    }

    [Function("TriggerLowStockCheck")]
    [OpenApiOperation(operationId: "TriggerLowStockCheck", tags: new[] { "Alerts" }, Summary = "Manually run the low-stock check and send an alert email if needed")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object), Description = "Count and list of low-stock products found")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "low-stock-check/run")] HttpRequestData req)
    {
        var lowStockProducts = await _checkService.RunAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { count = lowStockProducts.Count, products = lowStockProducts });
        return response;
    }
}
