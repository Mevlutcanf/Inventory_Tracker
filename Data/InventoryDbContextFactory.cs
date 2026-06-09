using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InventoryChecker.Data;

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var sqliteDatabasePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "inventory.db");
        var sqliteConnection = $"Data Source={sqliteDatabasePath}";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        }
        else
        {
            optionsBuilder.UseSqlite(sqliteConnection);
        }

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
