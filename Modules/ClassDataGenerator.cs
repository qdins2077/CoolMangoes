using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CoolMangoes.Modules
{
    public class ClassDataGenerator
    {
        public List<ClassData> GenerateClassDataTemplate(List<Asset> assetData)
        {
            var classDataList = assetData
                .GroupBy(a => a.HierarchyCode)
                .Select(g => new ClassData
                {
                    AssetHierarchy = g.FirstOrDefault()?.AssetHierarchy,
                    HierarchyCode = g.Key,
                    AssetType = null, // Set default or null values
                    MaintenanceType = null,
                    Statutory = null,
                    EstimatedLife = null,
                    RefurbishmentFrequency = null,
                    RefurbishmentCostAsProportionOfReplacementCost = null,
                    MinCost = null,
                    MaxCost = null,
                    AvgReplacementCost = null
                })
                .ToList();

            return classDataList;
        }

        public void DownloadClassDataTemplate(List<ClassData> classDataList, string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true
            };

            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, config))
            {
                csvWriter.Context.RegisterClassMap<ClassDataMap>();
                csvWriter.WriteRecords(classDataList);
            }
        }
    }

    
}
