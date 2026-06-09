using InventoryChecker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5055");

builder.Services.AddControllersWithViews();

// Authentication: simple cookie auth for admin access
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "InventoryAuth";
    });
builder.Services.AddAuthorization();

var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var sqliteDatabasePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "inventory.db");
Directory.CreateDirectory(Path.GetDirectoryName(sqliteDatabasePath)!);
var sqliteConnection = $"Data Source={sqliteDatabasePath}";
var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServer") ?? "Server=(localdb)\\MSSQLLocalDB;Database=InventoryChecker;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(sqlServerConnection);
    }
    else
    {
        options.UseSqlite(sqliteConnection);
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    // Apply any pending migrations and then run the seeder
    await context.Database.MigrateAsync();
    await InventorySeeder.InitializeAsync(context);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
