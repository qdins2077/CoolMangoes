using System;

namespace CoolMangoes.Models
{
    public class Asset
    {
        public string? Location1 { get; set; }
        public string? Location2 { get; set; }
        public string? Location3 { get; set; }
        public string? Location4 { get; set; }
        public string Asset_ID { get; set; } = string.Empty;
        public string? Parent_ID { get; set; }
        public string AssetDescription { get; set; } = string.Empty;
        public string? AssetHierarchy { get; set; }
        public string? HierarchyL1 { get; set; }
        public string? HierarchyL2 { get; set; }
        public string? HierarchyL3 { get; set; }
        public string? HierarchyL4 { get; set; }
        public string? HierarchyCode { get; set; }
        public string? Manufacturer { get; set; }
        public string? ModelNumber { get; set; }
        public string? ManufSerialNo { get; set; }
        public DateTime? AcqDate { get; set; }
        public int? ConditionRating { get; set; }
        public int? CurrentUsage { get; set; }
        public int? OperatingEnvironment { get; set; }
        public float? PurchaseCost { get; set; }
        public DateTime? ObservationDate { get; set; }
        public string? MaintenanceStrategyCode { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Statutory { get; set; }
        public int? Criticality { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
    }

    public class ClassData
    {
        public string AssetHierarchy { get; set; } = string.Empty;
        public string HierarchyCode { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string? Statutory { get; set; }
        public float? EstimatedLife { get; set; }
        public float? RefurbishmentFrequency { get; set; }
        public float? RefurbishmentCostAsProportionOfReplacementCost { get; set; }
        public float? MinCost { get; set; }
        public float? MaxCost { get; set; }
        public float? AvgReplacementCost { get; set; }
    }


    public class Expenditure
    {
        public int Asset_ID { get; set; }
        public string AssetDescription { get; set; } = string.Empty;
        public string HierarchyCode { get; set; } = string.Empty;
        public DateTime ExpenditureDate { get; set; }
        public double ExpenditureValue { get; set; }
        public string ExpenditureType { get; set; } = string.Empty; // Ensure it's not nullable
    }
}
