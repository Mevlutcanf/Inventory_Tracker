using System.ComponentModel.DataAnnotations;

namespace InventoryChecker.Models;

public class Asset
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Category { get; set; } = string.Empty;

    [StringLength(120)]
    public string? SerialNumber { get; set; }

    [StringLength(80)]
    public string? Brand { get; set; }

    [StringLength(80)]
    public string? Model { get; set; }

    [StringLength(80)]
    public string? Location { get; set; }

    public DateTime? PurchasedOn { get; set; }

    public DateTime? RetiredOn { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
