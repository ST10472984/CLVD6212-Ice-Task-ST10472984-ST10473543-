using System.Net;
using System.Text.Json;
using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Functions;

public class StockMovementsFunctions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly InventoryDbContext _db;
    private readonly ILogger<StockMovementsFunctions> _logger;

    public StockMovementsFunctions(InventoryDbContext db, ILogger<StockMovementsFunctions> logger)
    {
        _db = db;
        _logger = logger;
    }

    [Function("GetMovementsForProduct")]
    public async Task<HttpResponseData> GetMovementsForProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id:int}/movements")] HttpRequestData req,
        int id)
    {
        var movements = await _db.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == id)
            .OrderByDescending(m => m.TimestampUtc)
            .ToListAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(movements);
        return response;
    }

    [Function("CreateStockMovement")]
    public async Task<HttpResponseData> CreateStockMovement(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products/{id:int}/movements")] HttpRequestData req,
        int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteAsJsonAsync(new { error = "Product not found." });
            return notFound;
        }

        var payload = await JsonSerializer.DeserializeAsync<StockMovementRequest>(req.Body, JsonOptions);
        if (payload is null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid request body." });
            return badRequest;
        }

        var newStock = product.CurrentStock + payload.QuantityChange;
        if (newStock < 0)
        {
            var conflict = req.CreateResponse(HttpStatusCode.Conflict);
            await conflict.WriteAsJsonAsync(new { error = "Movement would result in negative stock." });
            return conflict;
        }

        var movement = new StockMovement
        {
            ProductId = product.Id,
            QuantityChange = payload.QuantityChange,
            Reason = payload.Reason,
            Note = payload.Note,
            StaffUser = payload.StaffUser
        };

        product.CurrentStock = newStock;
        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(new { movement, product.CurrentStock });
        return response;
    }
}
