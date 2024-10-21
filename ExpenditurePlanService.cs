using System;
using System.Collections.Generic;
using CoolMangoes.Models;
using CoolMangoes.Modules;

namespace CoolMangoes
{
    public class ExpenditurePlanService
    {
        
        public static List<Expenditure> Calculate(List<Asset> assetData, List<CoolMangoes.Models.ClassData> classDataList)
        {
            var classDataMap = new Dictionary<string, CoolMangoes.Models.ClassData>();
            foreach (var entry in classDataList)
            {
                classDataMap[entry.HierarchyCode] = entry;
            }

            var expenditurePlan = new List<Expenditure>();
            foreach (var asset in assetData)
            {
                if (string.IsNullOrWhiteSpace(asset.HierarchyCode)) continue;
                if (!classDataMap.TryGetValue(asset.HierarchyCode, out var classData)) continue;

                var plannedStartDate = asset.PlannedStartDate ?? DateTime.Now;
                var plannedEndDate = asset.PlannedEndDate ?? DateTime.Now.AddYears(10);
                var acquisitionDate = asset.AcqDate ?? DateTime.Now;

                expenditurePlan.AddRange(GenerateReplacementAndRefurbishmentExpenditure(
                    asset, classData, plannedStartDate, plannedEndDate, acquisitionDate));
            }

            return expenditurePlan;
        }

        private static IEnumerable<Expenditure> GenerateReplacementAndRefurbishmentExpenditure(Asset asset, CoolMangoes.Models.ClassData classData, DateTime startDate, DateTime endDate, DateTime acqDate)
        {
            var estimatedLife = classData.EstimatedLife ?? 0;
            var refurbishmentFrequency = classData.RefurbishmentFrequency ?? 0;

            // Using AvgReplacementCost
            var replacementCost = asset.PurchaseCost ?? classData.AvgReplacementCost ?? 0.0;

            DateTime nextReplacementDate = acqDate.AddYears((int)estimatedLife);
            DateTime nextRefurbishmentDate = acqDate.AddYears((int)refurbishmentFrequency);

            while (nextReplacementDate <= endDate)
            {
                yield return new Expenditure
                {
                    Asset_ID = int.Parse(asset.Asset_ID),
                    AssetDescription = asset.AssetDescription,
                    HierarchyCode = asset.HierarchyCode,
                    ExpenditureDate = nextReplacementDate,
                    ExpenditureValue = replacementCost,
                    ExpenditureType = "Replacement"
                };

                while (nextRefurbishmentDate < nextReplacementDate && nextRefurbishmentDate <= endDate)
                {
                    var refurbishmentCost = refurbishmentFrequency > 0
                        ? replacementCost * (classData.RefurbishmentCostAsProportionOfReplacementCost ?? 0.0)
                        : 0.0;

                    yield return new Expenditure
                    {
                        Asset_ID = int.Parse(asset.Asset_ID),
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
