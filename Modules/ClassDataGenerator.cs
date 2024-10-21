using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CoolMangoes.Modules;
using CoolMangoes.Models; // Ensure you're using the correct ClassData from Models

namespace CoolMangoes.Modules
{
    public class ClassDataGenerator
    {
        public static List<ClassData> GenerateClassDataTemplate(List<Asset> assetData)
        {
            var classMap = new Dictionary<string, ClassData>();
            var purchaseCosts = new Dictionary<string, List<double>>();

            // Iterate over each asset in assetData
            foreach (var asset in assetData)
            {
                var key = asset.HierarchyCode?.Trim();
                if (string.IsNullOrEmpty(key)) continue;

                // Initialize ClassData entry if the HierarchyCode doesn't already exist
                if (!classMap.ContainsKey(key))
                {
                    classMap[key] = new ClassData
                    {
                        AssetHierarchy = asset.AssetHierarchy ?? string.Empty,
                        HierarchyCode = key,
                        AssetType = GetAssetType(asset),
                        MaintenanceType = asset.MaintenanceType ?? string.Empty,
                        Statutory = asset.Statutory,  // Assuming it's a map, if not leave it as null
                        EstimatedLife = null,  // Leave empty as requested
                        RefurbishmentFrequency = null,  // Leave empty as requested
                        RefurbishmentCostAsProportionOfReplacementCost = null  // Leave empty as requested
                    };
                }

                // Collect PurchaseCost data for later processing (min/max/avg)
                if (asset.PurchaseCost.HasValue)
                {
                    if (!purchaseCosts.ContainsKey(key))
                    {
                        purchaseCosts[key] = new List<double>();
                    }
                    purchaseCosts[key].Add(asset.PurchaseCost.Value);
                }
            }

            // Now compute the MinCost, MaxCost, and AvgReplacementCost for each HierarchyCode
            foreach (var entry in purchaseCosts)
            {
                var key = entry.Key;
                var costs = entry.Value;

                if (costs.Count > 0)
                {
                    // Explicitly cast from double to float for MinCost, MaxCost, and AvgReplacementCost
                    classMap[key].MinCost = (float?)costs.Min();
                    classMap[key].MaxCost = (float?)costs.Max();

                    // If there's only one cost, use it as the AvgReplacementCost
                    if (costs.Count == 1)
                    {
                        classMap[key].AvgReplacementCost = (float?)costs[0];
                    }
                    else
                    {
                        // Average the Min and Max
                        classMap[key].AvgReplacementCost = (float?)((classMap[key].MinCost + classMap[key].MaxCost) / 2);
                    }
                }
            }

            // Return the sorted classData list
            var classData = classMap.Values.OrderBy(c => c.AssetHierarchy).ToList();
            return classData;
        }

        private static string GetAssetType(Asset asset)
        {
            return asset.AssetHierarchy?.Split('\\').LastOrDefault() ?? string.Empty;
        }

        public static void DownloadClassDataTemplate(List<ClassData> classData, string downloadPath)
        {
            var headers = new List<string>
            {
                "AssetHierarchy", "HierarchyCode", "AssetType", "MaintenanceType", "Statutory",
                "EstimatedLife", "RefurbishmentFrequency", "RefurbishmentCostAsProportionOfReplacementCost",
                "MinCost", "MaxCost", "AvgReplacementCost"
            };

            using (var writer = new StreamWriter(downloadPath))
            using (var csvWriter = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField(headers);
                csvWriter.NextRecord();

                foreach (var row in classData)
                {
                    csvWriter.WriteRecord(row);
                    csvWriter.NextRecord();
                }
            }
        }
    }
}
