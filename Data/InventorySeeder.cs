using InventoryChecker.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Data;

public static class InventorySeeder
{
    public static async Task InitializeAsync(InventoryDbContext context)
    {
        // Assume migrations have been applied. Seed only when there are no assets.
        if (await context.Assets.AnyAsync())
        {
            await BackfillDemoDatesAsync(context);
            return;
        }

        var employees = new[]
        {
            new Employee
            {
                FullName = "Ahmet Yılmaz",
                Department = "IT",
                Title = "Sistem Uzmanı",
                Email = "ahmet.yilmaz@example.com",
                Phone = "+90 555 100 20 30"
            },
            new Employee
            {
                FullName = "Elif Kaya",
                Department = "Finans",
                Title = "Uzman",
                Email = "elif.kaya@example.com",
                Phone = "+90 555 100 20 31"
            },
            new Employee
            {
                FullName = "Mert Demir",
                Department = "Satış",
                Title = "Satış Temsilcisi",
                Email = "mert.demir@example.com",
                Phone = "+90 555 100 20 32"
            }
        };

        var assets = new[]
        {
            new Asset
            {
                AssetTag = "LT-1001",
                Name = "Dell Latitude 5440",
                Category = "Laptop",
                Brand = "Dell",
                Model = "Latitude 5440",
                SerialNumber = "DL-5440-001",
                Location = "IT Depo",
                PurchasedOn = new DateTime(2025, 1, 10),
                Status = AssetStatus.Assigned,
                Notes = "Yönetici kullanımında."
            },
            new Asset
            {
                AssetTag = "PH-2001",
                Name = "iPhone 15",
                Category = "Telefon",
                Brand = "Apple",
                Model = "iPhone 15",
                SerialNumber = "IP-15-2001",
                Location = "Mobil Envanter",
                PurchasedOn = new DateTime(2025, 3, 2),
                Status = AssetStatus.Available
            },
            new Asset
            {
                AssetTag = "MN-3001",
                Name = "27\" Curved Monitor",
                Category = "Monitör",
                Brand = "Samsung",
                Model = "S27C",
                SerialNumber = "SM-S27C-9001",
                Location = "IT Depo",
                PurchasedOn = new DateTime(2024, 11, 15),
                Status = AssetStatus.Retired,
                RetiredOn = DateTime.UtcNow.Date.AddDays(-3)
            }
        };

        context.Employees.AddRange(employees);
        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();

        context.AssetAssignments.AddRange(
            new AssetAssignment
            {
                AssetId = assets[0].Id,
                EmployeeId = employees[0].Id,
                AssignedBy = "Sistem",
                AssignedOn = DateTime.UtcNow.AddDays(-20),
                Notes = "Kurulum tamamlandı."
            },
            new AssetAssignment
            {
                AssetId = assets[2].Id,
                EmployeeId = employees[1].Id,
                AssignedBy = "Sistem",
                AssignedOn = DateTime.UtcNow.AddDays(-45),
                ReturnedOn = DateTime.UtcNow.AddDays(-5),
                Notes = "Geçici test ataması."
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task BackfillDemoDatesAsync(InventoryDbContext context)
    {
        var now = DateTime.Today;

        var laptop = await context.Assets.FirstOrDefaultAsync(x => x.AssetTag == "LT-1001");
        if (laptop is not null && (laptop.PurchasedOn is null || laptop.PurchasedOn.Value.Month != now.Month || laptop.PurchasedOn.Value.Year != now.Year))
        {
            laptop.PurchasedOn = now.AddDays(-4);
        }

        var phone = await context.Assets.FirstOrDefaultAsync(x => x.AssetTag == "PH-2001");
        if (phone is not null && (phone.PurchasedOn is null || phone.PurchasedOn.Value.Month != now.Month || phone.PurchasedOn.Value.Year != now.Year))
        {
            phone.PurchasedOn = now.AddDays(-2);
        }

        var monitor = await context.Assets.FirstOrDefaultAsync(x => x.AssetTag == "MN-3001");
        if (monitor is not null)
        {
            monitor.Status = AssetStatus.Retired;
            monitor.RetiredOn = now.AddDays(-3);
            monitor.Location = "Çıkış Deposu";
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureSqliteSchemaAsync(InventoryDbContext context)
    {
        if (!context.Database.IsSqlite())
        {
            return;
        }

        var connection = (SqliteConnection)context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('Assets');";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var hasRetiredOn = false;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                if (reader.GetString(1).Equals("RetiredOn", StringComparison.OrdinalIgnoreCase))
                {
                    hasRetiredOn = true;
                    break;
                }
            }
        }

        if (!hasRetiredOn)
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE Assets ADD COLUMN RetiredOn TEXT NULL;";
            await alter.ExecuteNonQueryAsync();
        }
    }
}
