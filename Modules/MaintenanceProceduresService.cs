using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CoolMangoes.Modules
{
    public class MaintenanceProceduresService
    {
        // Generate MaintenanceProcedures based on MaintenanceStrategies and AssetData
        public List<MaintenanceProcedure> GenerateMaintenanceProcedures(List<MaintenanceStrategy> strategies, List<Asset> assets)
        {
            var procedures = new List<MaintenanceProcedure>();

            // Match StrategyCode from strategies to HierarchyCode from assets and create MaintenanceProcedures
            foreach (var strategy in strategies)
            {
                var matchingAssets = assets.Where(a => a.HierarchyCode == strategy.StrategyCode).DistinctBy(a => a.HierarchyCode).ToList();

                foreach (var asset in matchingAssets)
                {
                    var procedure = new MaintenanceProcedure
                    {
                        StrategyCode = strategy.StrategyCode,
                        StrategyDescription = strategy.StrategyDescription,
                        HierarchyL1 = asset.HierarchyL1,
                        HierarchyL2 = asset.HierarchyL2,
                        HierarchyL3 = asset.HierarchyL3,
                        HierarchyL4 = asset.HierarchyL4,
                        ProcedureCode = string.Empty,  // Empty by default, can be filled later
                        ProcedureDescription = string.Empty,  // Empty by default, can be filled later
                        Duration = null,  // Empty by default, can be filled later
                        Frequency = null,  // Empty by default, can be filled later
                        FrequencyType = string.Empty,  // Empty by default, can be filled later
                        MaintenanceStatus = string.Empty,  // Empty by default, can be filled later
                        Statutory = asset.Statutory,
                        LastDoneDate = null  // Empty by default, can be filled later
                    };

                    procedures.Add(procedure);
                }
            }

            return procedures;
        }

        // Method to download MaintenanceProcedures template to a CSV file
        public void DownloadMaintenanceProceduresTemplate(List<MaintenanceProcedure> procedures, string filePath)
        {
            var headers = new List<string>
            {
                "StrategyCode", "StrategyDescription", "HierarchyL1", "HierarchyL2", "HierarchyL3", "HierarchyL4",
                "ProcedureCode", "ProcedureDescription", "Duration", "Frequency", "FrequencyType", "MaintenanceStatus", 
                "Statutory", "LastDoneDate"
            };

            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write headers
                csvWriter.WriteField(headers);
                csvWriter.NextRecord();

                // Write each MaintenanceProcedure record
                foreach (var procedure in procedures)
                {
                    csvWriter.WriteRecord(procedure);
                    csvWriter.NextRecord();
                }
            }
        }

        // Method to upload and parse MaintenanceProcedures from a CSV file
        public List<MaintenanceProcedure> LoadMaintenanceProcedures(string filePath)
        {
            var proceduresList = new List<MaintenanceProcedure>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,  // Handle missing fields gracefully
                HeaderValidated = null,    // Disable header validation
                IgnoreBlankLines = true    // Ignore blank lines
            };

            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, config))
            {
                // Register class map for MaintenanceProcedure
                csvReader.Context.RegisterClassMap<MaintenanceProcedureMap>();
                proceduresList = csvReader.GetRecords<MaintenanceProcedure>().ToList();
            }

            return proceduresList;
        }
    }

    // MaintenanceProcedure class model
    public class MaintenanceProcedure
    {
        public string StrategyCode { get; set; } = string.Empty;
        public string StrategyDescription { get; set; } = string.Empty;
        public string? HierarchyL1 { get; set; }
        public string? HierarchyL2 { get; set; }
        public string? HierarchyL3 { get; set; }
        public string? HierarchyL4 { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureDescription { get; set; } = string.Empty;
        public float? Duration { get; set; }
        public float? Frequency { get; set; }
        public string FrequencyType { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = string.Empty;
        public string? Statutory { get; set; }
        public DateTime? LastDoneDate { get; set; }
    }

    // Class map for the MaintenanceProcedure CSV file structure
    public class MaintenanceProcedureMap : ClassMap<MaintenanceProcedure>
    {
        public MaintenanceProcedureMap()
        {
            Map(m => m.StrategyCode);
            Map(m => m.StrategyDescription);
            Map(m => m.HierarchyL1);
            Map(m => m.HierarchyL2);
            Map(m => m.HierarchyL3);
            Map(m => m.HierarchyL4);
            Map(m => m.ProcedureCode);
            Map(m => m.ProcedureDescription);
            Map(m => m.Duration);
            Map(m => m.Frequency);
            Map(m => m.FrequencyType);
            Map(m => m.MaintenanceStatus);
            Map(m => m.Statutory);
            Map(m => m.LastDoneDate);
        }
    }
}
