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

            foreach (var strategy in strategies)
            {
                var matchingAssets = assets.Where(a => a.HierarchyCode == strategy.StrategyCode).ToList();

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
                        ProcedureCode = string.Empty,
                        ProcedureDescription = string.Empty,
                        Duration = null,
                        Frequency = null,
                        FrequencyType = string.Empty,
                        MaintenanceStatus = string.Empty,
                        Statutory = asset.Statutory,
                        LastDoneDate = null
                    };

                    procedures.Add(procedure);
                }
            }

            return procedures;
        }

        // Method to download MaintenanceProcedures template to a CSV file
        public void DownloadMaintenanceProceduresTemplate(List<MaintenanceProcedure> procedures, string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true
            };

            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, config))
            {
                csvWriter.Context.RegisterClassMap<MaintenanceProcedureMap>();
                csvWriter.WriteRecords(procedures);
            }
        }

        // Method to upload and parse MaintenanceProcedures from a CSV file
        public List<MaintenanceProcedure> LoadMaintenanceProcedures(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = args => args.Header.Trim(),
            };

            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new NullableDateTimeConverter());
                csvReader.Context.TypeConverterCache.AddConverter<float?>(new NullableFloatConverter());

                csvReader.Context.RegisterClassMap<MaintenanceProcedureMap>();

                try
                {
                    var proceduresList = csvReader.GetRecords<MaintenanceProcedure>().ToList();
                    return proceduresList;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error reading maintenance procedures: {ex.Message}", ex);
                }
            }
        }
    }

    public class MaintenanceProcedureMap : ClassMap<MaintenanceProcedure>
    {
        public MaintenanceProcedureMap()
        {
            Map(m => m.StrategyCode);
            Map(m => m.StrategyDescription);
            Map(m => m.HierarchyL1).Optional();
            Map(m => m.HierarchyL2).Optional();
            Map(m => m.HierarchyL3).Optional();
            Map(m => m.HierarchyL4).Optional();
            Map(m => m.ProcedureCode);
            Map(m => m.ProcedureDescription);
            Map(m => m.Duration).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.Frequency).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.FrequencyType);
            Map(m => m.MaintenanceStatus);
            Map(m => m.Statutory).Optional();
            Map(m => m.LastDoneDate).TypeConverter<NullableDateTimeConverter>().Optional();
        }
    }
}
