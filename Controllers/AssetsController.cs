using InventoryChecker.Data;
using InventoryChecker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class AssetsController : Controller
{
    private readonly InventoryDbContext _context;

    public AssetsController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, AssetStatus? status)
    {
        var query = _context.Assets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.AssetTag.Contains(q) ||
                x.Name.Contains(q) ||
                x.Category.Contains(q) ||
                (x.SerialNumber != null && x.SerialNumber.Contains(q)) ||
                (x.Brand != null && x.Brand.Contains(q)) ||
                (x.Model != null && x.Model.Contains(q)) ||
                (x.Location != null && x.Location.Contains(q)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var assets = await query
            .OrderBy(x => x.AssetTag)
            .ToListAsync();

        ViewData["Query"] = q;
        ViewData["Status"] = status;

        return View(assets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var asset = await _context.Assets
            .Include(x => x.Assignments)
                .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (asset is null)
        {
            return NotFound();
        }

        return View(asset);
    }

    public IActionResult Create()
    {
        return View(new Asset { PurchasedOn = DateTime.Today, Status = AssetStatus.Available });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Asset asset)
    {
        if (!ModelState.IsValid)
        {
            return View(asset);
        }

        if (asset.Status == AssetStatus.Retired)
        {
            asset.RetiredOn ??= DateTime.Today;
        }

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset is null)
        {
            return NotFound();
        }

        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Asset asset)
    {
        if (id != asset.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(asset);
        }

        if (asset.Status == AssetStatus.Retired)
        {
            asset.RetiredOn ??= DateTime.Today;
        }
        else
        {
            asset.RetiredOn = null;
        }

        _context.Update(asset);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Id == id);
        if (asset is null)
        {
            return NotFound();
        }

        return View(asset);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset is not null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
