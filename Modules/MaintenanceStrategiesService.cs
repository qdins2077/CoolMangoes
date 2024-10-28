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
        // Generate the MaintenanceStrategies data from ClassData
        public List<MaintenanceStrategy> GenerateMaintenanceStrategies(List<ClassData> classDataList)
        {
            var maintenanceStrategies = classDataList
                .Where(c => c.MaintenanceType == "Planned")
                .OrderBy(c => c.HierarchyCode)
                .Select(classData => new MaintenanceStrategy
                {
                    StrategyCode = classData.HierarchyCode,
                    StrategyDescription = classData.AssetType,
                    CostPerHour = 0, // Default value, can be updated
                    ResourceType = string.Empty,
                    ResourceName = string.Empty
                })
                .ToList();

            return maintenanceStrategies;
        }

        // Method to download the MaintenanceStrategies template
        public void DownloadMaintenanceStrategiesTemplate(List<MaintenanceStrategy> maintenanceStrategies, string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true
            };

            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, config))
            {
                csvWriter.Context.RegisterClassMap<MaintenanceStrategyMap>();
                csvWriter.WriteRecords(maintenanceStrategies);
            }
        }

        // Method to load MaintenanceStrategies from an uploaded CSV file
        public List<MaintenanceStrategy> LoadMaintenanceStrategies(string filePath)
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
                csvReader.Context.TypeConverterCache.AddConverter<float?>(new NullableFloatConverter());

                csvReader.Context.RegisterClassMap<MaintenanceStrategyMap>();

                try
                {
                    var strategiesList = csvReader.GetRecords<MaintenanceStrategy>().ToList();
                    return strategiesList;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error reading maintenance strategies: {ex.Message}", ex);
                }
            }
        }
    }

    public class MaintenanceStrategyMap : ClassMap<MaintenanceStrategy>
    {
        public MaintenanceStrategyMap()
        {
            Map(m => m.StrategyCode);
            Map(m => m.StrategyDescription);
            Map(m => m.CostPerHour).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.ResourceType).Optional();
            Map(m => m.ResourceName).Optional();
        }
    }
}
