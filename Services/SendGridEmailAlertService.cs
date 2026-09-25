using InventoryTracker.Functions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace InventoryTracker.Functions.Services;

public class SendGridEmailAlertService : IEmailAlertService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailAlertService> _logger;

    public SendGridEmailAlertService(IConfiguration configuration, ILogger<SendGridEmailAlertService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendLowStockAlertAsync(IReadOnlyList<Product> lowStockProducts, CancellationToken cancellationToken = default)
    {
        if (lowStockProducts.Count == 0)
        {
            return;
        }

        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["Alert:FromEmail"];
        var toEmail = _configuration["Alert:ToEmail"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("SendGrid configuration is missing; skipping low-stock email alert.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "Inventory Tracker");
        var to = new EmailAddress(toEmail);
        var subject = $"Low stock alert: {lowStockProducts.Count} item(s) need reordering";
        var body = "The following items are at or below their reorder threshold:\n\n" +
            string.Join("\n", lowStockProducts.Select(p =>
                $"- {p.Name} (SKU: {p.Sku}): {p.CurrentStock} {p.Unit} remaining, threshold {p.ReorderThreshold}"));

        var message = MailHelper.CreateSingleEmail(from, to, subject, body, htmlContent: null);
        var response = await client.SendEmailAsync(message, cancellationToken);

        var statusCode = (int)response.StatusCode;
        if (statusCode < 200 || statusCode >= 300)
        {
            _logger.LogWarning("SendGrid returned status {StatusCode} when sending low-stock alert.", response.StatusCode);
        }
    }
}
