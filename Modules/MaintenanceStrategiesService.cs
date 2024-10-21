using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CoolMangoes.Modules
{
    public class MaintenanceStrategiesService
    {
        private List<MaintenanceStrategy> strategiesList = new List<MaintenanceStrategy>();

        // Generate the MaintenanceStrategies data from ClassData
        public List<MaintenanceStrategy> GenerateMaintenanceStrategies(List<ClassData> classDataList)
        {
            var maintenanceStrategies = new List<MaintenanceStrategy>();

            // Filter and map the relevant ClassData where MaintenanceType = "Planned"
            var filteredData = classDataList
                .Where(c => c.MaintenanceType == "Planned")
                .OrderBy(c => c.HierarchyCode) // Sort the HierarchyCode
                .ToList();

            foreach (var classData in filteredData)
            {
                var strategy = new MaintenanceStrategy
                {
                    StrategyCode = classData.HierarchyCode,
                    StrategyDescription = classData.AssetType,
                    CostPerHour = 0,  // Assuming default cost value, can be filled later
                    ResourceType = string.Empty,  // Default empty value, can be updated
                    ResourceName = string.Empty   // Default empty value, can be updated
                };

                maintenanceStrategies.Add(strategy);
            }

            // Store strategies in the internal list
            strategiesList = maintenanceStrategies;
            return maintenanceStrategies;
        }

        // Method to download the MaintenanceStrategies template
        public void DownloadMaintenanceStrategiesTemplate(List<MaintenanceStrategy> maintenanceStrategies, string filePath)
        {
            var headers = new List<string>
            {
                "StrategyCode", "StrategyDescription", "Cost/hour", "ResourceType", "ResourceName"
            };

            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write the headers
                csvWriter.WriteField(headers);
                csvWriter.NextRecord();

                // Write each MaintenanceStrategy record
                foreach (var strategy in maintenanceStrategies)
                {
                    csvWriter.WriteRecord(strategy);
                    csvWriter.NextRecord();
                }
            }
        }

        // Method to load MaintenanceStrategies from an uploaded CSV file
        public List<MaintenanceStrategy> LoadMaintenanceStrategies(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,  // Handle missing fields gracefully
                HeaderValidated = null,    // Disable header validation
                IgnoreBlankLines = true    // Ignore blank lines
            };

            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, config))
            {
                // Register class map for MaintenanceStrategy
                csvReader.Context.RegisterClassMap<MaintenanceStrategyMap>();
                strategiesList = csvReader.GetRecords<MaintenanceStrategy>().ToList();
            }

            return strategiesList;
        }

        // Method to get the current MaintenanceStrategiesList
        public List<MaintenanceStrategy> GetMaintenanceStrategiesList()
        {
            return strategiesList;
        }
    }

    // MaintenanceStrategy class model
    public class MaintenanceStrategy
    {
        public string StrategyCode { get; set; } = string.Empty;
        public string StrategyDescription { get; set; } = string.Empty;
        public float CostPerHour { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
    }

    // Class map for the MaintenanceStrategy CSV file structure
    public class MaintenanceStrategyMap : ClassMap<MaintenanceStrategy>
    {
        public MaintenanceStrategyMap()
        {
            Map(m => m.StrategyCode);
            Map(m => m.StrategyDescription);
            Map(m => m.CostPerHour);
            Map(m => m.ResourceType);
            Map(m => m.ResourceName);
        }
    }
}
