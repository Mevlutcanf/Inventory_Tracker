using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryChecker.ViewModels;

public class AssignmentCreateViewModel
{
    [Required]
    public int AssetId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required, StringLength(150)]
    public string AssignedBy { get; set; } = "IT Admin";

    [StringLength(1000)]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> AvailableAssets { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Employees { get; set; } = Enumerable.Empty<SelectListItem>();
}
