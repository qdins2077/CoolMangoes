using System;
using System.Collections.Generic;

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
        public string? UnderWarranty { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? Injury { get; set; }
        public int? Environmental { get; set; }
        public int? BusinessContinuity { get; set; }
        public int? Reputation { get; set; }
        public int? LossImpactOnTheCompany { get; set; }
        public int? HighestCriticality { get; set; }
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
        public int? Injury { get; set; }
        public int? Environmental { get; set; }
        public int? BusinessContinuity { get; set; }
        public int? Reputation { get; set; }
        public int? LossImpactOnTheCompany { get; set; }
        public int? HighestCriticality { get; set; }
    }


    public class Expenditure
    {
        public string Location1 { get; set; }
        public string Location2 { get; set; }
        public string Location3 { get; set; }
        public string Location4 { get; set; }
        public string Asset_ID { get; set; }
        public string AssetDescription { get; set; }
        public string HierarchyL1 { get; set; }
        public string HierarchyL2 { get; set; }
        public string HierarchyL3 { get; set; }
        public string HierarchyL4 { get; set; }
        public string AssetHierarchy { get; set; }
        public string HierarchyCode { get; set; }
        public double ExpenditureValue { get; set; }
        public DateTime? AcqDate { get; set; }
        public DateTime ExpenditureDate { get; set; }
        public int ExpenditureYear  { get; set; }
        public string ExpenditureType { get; set; }
        public string ExpenditureDescription { get; set; } 
        public string Comment { get; set; }
        public double? RUL { get; set; }
        public int? LoF { get; set; }
    }

    public class MaintenanceStrategy
    {
        public string StrategyCode { get; set; }
        public string StrategyDescription { get; set; }
        public float? CostPerHour { get; set; }
        public string ResourceType { get; set; }
        public string ResourceName { get; set; }
    }
    public class MaintenanceProcedure
    {
        public string StrategyCode { get; set; }
        public string StrategyDescription { get; set; }
        public string HierarchyL1 { get; set; }
        public string HierarchyL2 { get; set; }
        public string HierarchyL3 { get; set; }
        public string HierarchyL4 { get; set; }
        public string ProcedureCode { get; set; }
        public string ProcedureDescription { get; set; }
        public float? Duration { get; set; }
        public float? Frequency { get; set; }
        public string FrequencyType { get; set; }
        public string MaintenanceStatus { get; set; }
        public string Statutory { get; set; }
        public DateTime? LastDoneDate { get; set; }
    }

    public class CapitalProject
    {
        public string Location1 { get; set; }
        public string Location2 { get; set; }
        public string Location3 { get; set; }
        public string Location4 { get; set; }
        public string Asset_ID { get; set; }
        public string ProjectCategory { get; set; }
        public string ProjectTitle { get; set; }
        public double ProjectCost { get; set; }
        public int ProjectStartYear { get; set; }
        public int ProjectEndYear { get; set; }
        public string HierarchyL1 { get; set; }
        public string HierarchyL2 { get; set; }
        public string HierarchyL3 { get; set; }
        public string HierarchyL4 { get; set; }
        public string AssetHierarchy { get; set; }
        public string HierarchyCode { get; set; }
    }

    public class HierarchyItem
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Parent_ID { get; set; }
        public List<HierarchyItem> Children { get; set; } = new List<HierarchyItem>();
        public int Level { get; set; }
         public bool IsAssetOrParent { get; set; }
    }

}
