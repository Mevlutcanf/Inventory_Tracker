using InventoryChecker.Data;
using InventoryChecker.Models;
using InventoryChecker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Controllers;

public class HomeController : Controller
{
    private readonly InventoryDbContext _context;

    public HomeController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, int? employeeId, string? department)
    {
        var now = DateTime.Today;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var activeAssignmentsQuery = _context.AssetAssignments
            .Include(x => x.Asset)
            .Include(x => x.Employee)
            .Where(x => x.ReturnedOn == null);

        if (!string.IsNullOrWhiteSpace(q))
        {
            activeAssignmentsQuery = activeAssignmentsQuery.Where(x =>
                x.Asset.AssetTag.Contains(q) ||
                x.Asset.Name.Contains(q) ||
                x.Employee.FullName.Contains(q) ||
                (x.Employee.Department != null && x.Employee.Department.Contains(q)));
        }

        if (employeeId.HasValue)
        {
            activeAssignmentsQuery = activeAssignmentsQuery.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            activeAssignmentsQuery = activeAssignmentsQuery.Where(x => x.Employee.Department == department);
        }

        var activeAssignments = await activeAssignmentsQuery
            .OrderByDescending(x => x.AssignedOn)
            .Take(6)
            .Select(x => new DashboardAssignmentItemViewModel
            {
                AssetTag = x.Asset.AssetTag,
                AssetName = x.Asset.Name,
                EmployeeName = x.Employee.FullName,
                Department = x.Employee.Department ?? string.Empty,
                AssignedOn = x.AssignedOn
            })
            .ToListAsync();

        var model = new DashboardViewModel
        {
            AssetCount = await _context.Assets.CountAsync(),
            EmployeeCount = await _context.Employees.CountAsync(),
            AssignedAssetCount = await _context.Assets.CountAsync(x => x.Status == AssetStatus.Assigned),
            AvailableAssetCount = await _context.Assets.CountAsync(x => x.Status == AssetStatus.Available),
            MonthlyAddedAssets = await _context.Assets.CountAsync(x => x.PurchasedOn.HasValue && x.PurchasedOn.Value >= monthStart && x.PurchasedOn.Value < nextMonth),
            MonthlyDisposedAssets = await _context.Assets.CountAsync(x => x.RetiredOn.HasValue && x.RetiredOn.Value >= monthStart && x.RetiredOn.Value < nextMonth),
            CurrentMonthLabel = now.ToString("MMMM yyyy"),
            ActiveAssignments = activeAssignments
        };

        ViewData["Query"] = q;
        ViewData["EmployeeId"] = employeeId;
        ViewData["Department"] = department;
        ViewBag.Employees = await _context.Employees.OrderBy(x => x.FullName).ToListAsync();
        ViewBag.Departments = await _context.Employees
            .Where(x => x.Department != null)
            .Select(x => x.Department!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return View(model);
    }
}
