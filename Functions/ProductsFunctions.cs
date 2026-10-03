using System.Net;
using System.Text.Json;
using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
    [OpenApiOperation(operationId: "GetProducts", tags: new[] { "Products" }, Summary = "List all products")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<Product>), Description = "All products")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var products = await _db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        return await JsonResponse(req, HttpStatusCode.OK, products);
    }

    [Function("GetLowStockProducts")]
    [OpenApiOperation(operationId: "GetLowStockProducts", tags: new[] { "Products" }, Summary = "List products at or below their reorder threshold")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<Product>), Description = "Low-stock products")]
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
    [OpenApiOperation(operationId: "GetProductById", tags: new[] { "Products" }, Summary = "Get a single product by id")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Product), Description = "The product")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
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
    [OpenApiOperation(operationId: "CreateProduct", tags: new[] { "Products" }, Summary = "Create a new product")]
    [OpenApiRequestBody("application/json", typeof(ProductCreateRequest), Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Product), Description = "The created product")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Name or Sku missing")]
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
    [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "Products" }, Summary = "Update an existing product")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int))]
    [OpenApiRequestBody("application/json", typeof(ProductUpdateRequest), Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Product), Description = "The updated product")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
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
    [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "Products" }, Summary = "Delete a product")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int))]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
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
