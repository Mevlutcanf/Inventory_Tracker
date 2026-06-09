using InventoryChecker.Data;
using InventoryChecker.Models;
using InventoryChecker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class AssignmentsController : Controller
{
    private readonly InventoryDbContext _context;

    public AssignmentsController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, bool? activeOnly, int? employeeId, string? department)
    {
        var query = _context.AssetAssignments
            .Include(x => x.Asset)
            .Include(x => x.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.Asset.AssetTag.Contains(q) ||
                x.Asset.Name.Contains(q) ||
                x.Employee.FullName.Contains(q) ||
                (x.Employee.Department != null && x.Employee.Department.Contains(q)));
        }

        if (activeOnly == true)
        {
            query = query.Where(x => x.ReturnedOn == null);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(x => x.Employee.Department == department);
        }

        var assignments = await query
            .OrderByDescending(x => x.AssignedOn)
            .ToListAsync();

        ViewData["Query"] = q;
        ViewData["ActiveOnly"] = activeOnly;
        ViewData["EmployeeId"] = employeeId;
        ViewData["Department"] = department;

        // provide lists for filter selects
        ViewBag.Employees = await _context.Employees
            .OrderBy(x => x.FullName)
            .ToListAsync();

        ViewBag.Departments = await _context.Employees
            .Where(x => x.Department != null)
            .Select(x => x.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return View(assignments);
    }

    public async Task<IActionResult> Create()
    {
        var model = new AssignmentCreateViewModel
        {
            AvailableAssets = await GetAvailableAssetsAsync(),
            Employees = await GetEmployeesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableAssets = await GetAvailableAssetsAsync();
            model.Employees = await GetEmployeesAsync();
            return View(model);
        }

        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Id == model.AssetId);
        if (asset is null || asset.Status == AssetStatus.Assigned)
        {
            ModelState.AddModelError(string.Empty, "Seçilen zimmet zaten atanmış durumda.");
            model.AvailableAssets = await GetAvailableAssetsAsync();
            model.Employees = await GetEmployeesAsync();
            return View(model);
        }

        var assignment = new AssetAssignment
        {
            AssetId = model.AssetId,
            EmployeeId = model.EmployeeId,
            AssignedBy = model.AssignedBy,
            Notes = model.Notes,
            AssignedOn = DateTime.UtcNow
        };

        asset.Status = AssetStatus.Assigned;
        asset.Location = "Zimmette";

        _context.AssetAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Return(int id)
    {
        var assignment = await _context.AssetAssignments
            .Include(x => x.Asset)
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (assignment is null)
        {
            return NotFound();
        }

        return View(assignment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnConfirmed(int id)
    {
        var assignment = await _context.AssetAssignments
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (assignment is null)
        {
            return NotFound();
        }

        assignment.ReturnedOn = DateTime.UtcNow;
        assignment.Asset.Status = AssetStatus.Available;
        assignment.Asset.Location = "IT Depo";

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetAvailableAssetsAsync()
    {
        return await _context.Assets
            .Where(x => x.Status != AssetStatus.Assigned)
            .OrderBy(x => x.AssetTag)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.AssetTag} - {x.Name}"
            })
            .ToListAsync();
    }

    private async Task<IEnumerable<SelectListItem>> GetEmployeesAsync()
    {
        return await _context.Employees
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.FullName} ({x.Department ?? "Departman yok"})"
            })
            .ToListAsync();
    }
}
