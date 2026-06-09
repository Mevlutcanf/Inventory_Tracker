using System.ComponentModel.DataAnnotations;

namespace InventoryChecker.Models;

public class AssetAssignment
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public Asset Asset { get; set; } = null!;

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required, StringLength(150)]
    public string AssignedBy { get; set; } = string.Empty;

    public DateTime AssignedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedOn { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
