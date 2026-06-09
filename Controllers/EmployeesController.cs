using InventoryChecker.Data;
using InventoryChecker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class EmployeesController : Controller
{
    private readonly InventoryDbContext _context;

    public EmployeesController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, bool? active)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.FullName.Contains(q) ||
                (x.Department != null && x.Department.Contains(q)) ||
                (x.Title != null && x.Title.Contains(q)) ||
                (x.Email != null && x.Email.Contains(q)));
        }

        if (active.HasValue)
        {
            query = query.Where(x => x.IsActive == active.Value);
        }

        var employees = await query
            .OrderBy(x => x.FullName)
            .ToListAsync();

        ViewData["Query"] = q;
        ViewData["Active"] = active;

        return View(employees);
    }

    public IActionResult Create()
    {
        return View(new Employee { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(employee);
        }

        _context.Update(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is not null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Assignments!)
                .ThenInclude(a => a.Asset)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        var totalAssignments = employee.Assignments.Count;
        var activeAssignments = employee.Assignments.Count(a => a.ReturnedOn is null);

        var byCategory = employee.Assignments
            .Where(a => a.Asset != null)
            .GroupBy(a => a.Asset.Category)
            .Select(g => new {
                Category = g.Key,
                Total = g.Count(),
                Active = g.Count(a => a.ReturnedOn is null)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        ViewData["TotalAssignments"] = totalAssignments;
        ViewData["ActiveAssignments"] = activeAssignments;
        ViewData["ByCategory"] = byCategory;

        return View(employee);
    }
}
