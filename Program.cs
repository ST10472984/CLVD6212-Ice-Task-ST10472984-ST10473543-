using InventoryTracker.Functions.Data;
using InventoryTracker.Functions.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddSingleton<IEmailAlertService, SendGridEmailAlertService>();
    })
    .Build();

// Demo/local convenience only: ensures the schema exists without requiring EF migrations.
// For a real deployment, replace this with `dotnet ef migrations` + `context.Database.Migrate()`.
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.EnsureCreated();
}

host.Run();
