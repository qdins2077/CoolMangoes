using System;
using System.Collections.Generic;
using CoolMangoes.Models;

namespace CoolMangoes.Modules
{
    public class ExpenditurePlanService
    {
        private readonly List<Asset> _assetDataList;
        private readonly Dictionary<string, ClassData> _classDataList;  // Dictionary for fast lookups
        private readonly List<MaintenanceProcedure> _procedureDataList;
        private readonly DateTime _projectStartDate;
        private readonly DateTime _projectEndDate;

        // Constructor to initialize data
        public ExpenditurePlanService(List<Asset> assetData, List<ClassData> classData, List<MaintenanceProcedure> procedureData, DateTime projectStartDate, DateTime projectEndDate)
        {
            _assetDataList = assetData;
            _procedureDataList = procedureData;
            _projectStartDate = projectStartDate;
            _projectEndDate = projectEndDate;

            // Convert classData list to a dictionary for fast lookups by HierarchyCode
            _classDataList = new Dictionary<string, ClassData>();
            foreach (var entry in classData)
            {
                if (!string.IsNullOrWhiteSpace(entry.HierarchyCode))
                {
                    _classDataList[entry.HierarchyCode] = entry;
                }
            }
        }

        // Generates the expenditure plan lazily to avoid memory overflow
        public IEnumerable<Expenditure> GenerateExpenditurePlan()
        {
            foreach (var asset in _assetDataList)
            {
                if (string.IsNullOrWhiteSpace(asset.HierarchyCode))
                {
                    Console.WriteLine($"Skipping asset {asset.Asset_ID} due to missing HierarchyCode.");
                    continue;  // Skip assets with no HierarchyCode
                }

                // Find matching class data for the asset
                if (!_classDataList.TryGetValue(asset.HierarchyCode, out var classData))
                {
                    Console.WriteLine($"Skipping asset {asset.Asset_ID} due to missing class data.");
                    continue;  // Skip if no matching class data
                }

                // Use asset's PlannedStartDate and PlannedEndDate if available, otherwise use project dates
                var plannedStartDate = asset.PlannedStartDate ?? _projectStartDate;
                var plannedEndDate = asset.PlannedEndDate ?? _projectEndDate;
                var acquisitionDate = asset.AcqDate ?? DateTime.Now;

                // Skip assets if neither EstimatedLife nor RefurbishmentFrequency are valid
                if ((classData.EstimatedLife ?? 0) <= 0 && (classData.RefurbishmentFrequency ?? 0) <= 0)
                {
                    Console.WriteLine($"Skipping asset {asset.Asset_ID} due to no valid Estimated Life or Refurbishment Frequency.");
                    continue;
                }

                // Generate replacement and refurbishment expenditures for the asset lazily
                foreach (var expenditure in GenerateReplacementAndRefurbishmentExpenditure(asset, classData, plannedStartDate, plannedEndDate, acquisitionDate))
                {
                    yield return expenditure;
                }
            }
        }

        // Generates replacement and refurbishment expenditures for a specific asset lazily
        private static IEnumerable<Expenditure> GenerateReplacementAndRefurbishmentExpenditure(Asset asset, ClassData classData, DateTime startDate, DateTime endDate, DateTime acqDate)
        {
            var estimatedLife = classData.EstimatedLife ?? 0;  // Get the estimated life of the asset
            var refurbishmentFrequency = classData.RefurbishmentFrequency ?? 0;  // Get the refurbishment frequency
            var replacementCost = asset.PurchaseCost ?? classData.AvgReplacementCost ?? 0.0;  // Determine replacement cost

            DateTime nextReplacementDate = acqDate.AddYears((int)estimatedLife);
            DateTime nextRefurbishmentDate = acqDate.AddYears((int)refurbishmentFrequency);

            // Generate replacement and refurbishment expenditures until the end date is reached
            while (nextReplacementDate <= endDate)
            {
                // Generate replacement expenditure
                yield return new Expenditure
                {
                    Asset_ID = asset.Asset_ID,  // Treat Asset_ID as a string
                    AssetDescription = asset.AssetDescription,
                    HierarchyCode = asset.HierarchyCode,
                    ExpenditureDate = nextReplacementDate,
                    ExpenditureValue = replacementCost,
                    ExpenditureType = "Replacement"
                };

                // Generate refurbishment expenditures that occur before the next replacement
                while (nextRefurbishmentDate < nextReplacementDate && nextRefurbishmentDate <= endDate)
                {
                    var refurbishmentCost = refurbishmentFrequency > 0
                        ? replacementCost * (classData.RefurbishmentCostAsProportionOfReplacementCost ?? 0.0)
                        : 0.0;

                    yield return new Expenditure
                    {
                        Asset_ID = asset.Asset_ID,  // Treat Asset_ID as a string
                        AssetDescription = asset.AssetDescription,
                        HierarchyCode = asset.HierarchyCode,
                        ExpenditureDate = nextRefurbishmentDate,
                        ExpenditureValue = refurbishmentCost,
                        ExpenditureType = "Refurbishment"
                    };

                    nextRefurbishmentDate = nextRefurbishmentDate.AddYears((int)refurbishmentFrequency);
                }

                nextReplacementDate = nextReplacementDate.AddYears((int)estimatedLife);
            }
        }
    }
}
