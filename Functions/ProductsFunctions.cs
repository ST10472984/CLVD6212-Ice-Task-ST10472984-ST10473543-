using System.Net;
using System.Text.Json;
using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Functions.Functions;

public class ProductsFunctions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly InventoryDbContext _db;
    private readonly ILogger<ProductsFunctions> _logger;

    public ProductsFunctions(InventoryDbContext db, ILogger<ProductsFunctions> logger)
    {
        _db = db;
        _logger = logger;
    }

    [Function("GetProducts")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var products = await _db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        return await JsonResponse(req, HttpStatusCode.OK, products);
    }

    [Function("GetLowStockProducts")]
    public async Task<HttpResponseData> GetLowStockProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/low-stock")] HttpRequestData req)
    {
        var lowStock = await _db.Products.AsNoTracking()
            .Where(p => p.CurrentStock <= p.ReorderThreshold)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return await JsonResponse(req, HttpStatusCode.OK, lowStock);
    }

    [Function("GetProductById")]
    public async Task<HttpResponseData> GetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return await JsonResponse(req, HttpStatusCode.NotFound, new { error = "Product not found." });
        }
        return await JsonResponse(req, HttpStatusCode.OK, product);
    }

    [Function("CreateProduct")]
    public async Task<HttpResponseData> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<ProductCreateRequest>(req.Body, JsonOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Name) || string.IsNullOrWhiteSpace(payload.Sku))
        {
            return await JsonResponse(req, HttpStatusCode.BadRequest, new { error = "Name and Sku are required." });
        }

        var product = new Product
        {
            Name = payload.Name,
            Sku = payload.Sku,
            Category = payload.Category,
            Unit = string.IsNullOrWhiteSpace(payload.Unit) ? "each" : payload.Unit,
            CurrentStock = payload.CurrentStock,
            ReorderThreshold = payload.ReorderThreshold
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return await JsonResponse(req, HttpStatusCode.Created, product);
    }

    [Function("UpdateProduct")]
    public async Task<HttpResponseData> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return await JsonResponse(req, HttpStatusCode.NotFound, new { error = "Product not found." });
        }

        var payload = await JsonSerializer.DeserializeAsync<ProductUpdateRequest>(req.Body, JsonOptions);
        if (payload is null)
        {
            return await JsonResponse(req, HttpStatusCode.BadRequest, new { error = "Invalid request body." });
        }

        product.Name = payload.Name;
        product.Category = payload.Category;
        product.Unit = payload.Unit;
        product.ReorderThreshold = payload.ReorderThreshold;

        await _db.SaveChangesAsync();
        return await JsonResponse(req, HttpStatusCode.OK, product);
    }

    [Function("DeleteProduct")]
    public async Task<HttpResponseData> DeleteProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return await JsonResponse(req, HttpStatusCode.NotFound, new { error = "Product not found." });
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseData> JsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T body)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(body);
        return response;
    }
}
