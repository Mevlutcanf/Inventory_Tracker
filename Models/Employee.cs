using System.ComponentModel.DataAnnotations;

namespace InventoryChecker.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Department { get; set; }

    [StringLength(120)]
    public string? Title { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(40)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
