namespace InventoryChecker.ViewModels;

public class DashboardViewModel
{
    public int AssetCount { get; set; }

    public int EmployeeCount { get; set; }

    public int AssignedAssetCount { get; set; }

    public int AvailableAssetCount { get; set; }

    public int MonthlyAddedAssets { get; set; }

    public int MonthlyDisposedAssets { get; set; }

    public string CurrentMonthLabel { get; set; } = string.Empty;

    public IReadOnlyList<DashboardAssignmentItemViewModel> ActiveAssignments { get; set; } = Array.Empty<DashboardAssignmentItemViewModel>();
}

public class DashboardAssignmentItemViewModel
{
    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTime AssignedOn { get; set; }
}
