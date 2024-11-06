using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CoolMangoes.Modules
{
    public class AssetDataService
    {
        public List<Asset> LoadAssetData(string filePath)
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
                // Register custom converters and class map
                csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new NullableDateTimeConverter());
                csvReader.Context.TypeConverterCache.AddConverter<float?>(new NullableFloatConverter());

                csvReader.Context.RegisterClassMap<AssetMap>();

                try
                {
                    var assetDataList = csvReader.GetRecords<Asset>().ToList();

                    // Filter out records missing required fields
                    assetDataList = assetDataList
                        .Where(record => !string.IsNullOrWhiteSpace(record.Asset_ID) && !string.IsNullOrWhiteSpace(record.AssetDescription))
                        .ToList();

                    return assetDataList;
                }
                catch (Exception ex)
                {
                    // Handle exceptions appropriately (e.g., log the error)
                    // For now, rethrow the exception
                    throw new Exception($"Error reading asset data: {ex.Message}", ex);
                }
            }
        }
    }

    public class AssetMap : ClassMap<Asset>
    {
        public AssetMap()
        {
            // Map fields with optional configuration
            Map(m => m.Location1).Optional();
            Map(m => m.Location2).Optional();
            Map(m => m.Location3).Optional();
            Map(m => m.Location4).Optional();
            Map(m => m.Asset_ID).Name("Asset_ID");
            Map(m => m.Parent_ID).Optional();
            Map(m => m.AssetDescription).Name("AssetDescription");
            Map(m => m.AssetHierarchy).Optional();
            Map(m => m.HierarchyL1).Optional();
            Map(m => m.HierarchyL2).Optional();
            Map(m => m.HierarchyL3).Optional();
            Map(m => m.HierarchyL4).Optional();
            Map(m => m.HierarchyCode).Optional();
            Map(m => m.Manufacturer).Optional();
            Map(m => m.ModelNumber).Optional();
            Map(m => m.ManufSerialNo).Optional();
            Map(m => m.AcqDate).TypeConverter<NullableDateTimeConverter>().Optional();
            Map(m => m.ConditionRating).Optional();
            Map(m => m.CurrentUsage).Optional();
            Map(m => m.OperatingEnvironment).Optional();
            Map(m => m.PurchaseCost).TypeConverter<NullableFloatConverter>().Optional();
            Map(m => m.ObservationDate).TypeConverter<NullableDateTimeConverter>().Optional();
            Map(m => m.MaintenanceStrategyCode).Optional();
            Map(m => m.MaintenanceType).Optional();
            Map(m => m.Statutory).Optional();
            Map(m => m.PlannedStartDate).TypeConverter<NullableDateTimeConverter>().Optional();
            Map(m => m.PlannedEndDate).TypeConverter<NullableDateTimeConverter>().Optional();
            Map(m => m.Injury).Optional();
            Map(m => m.Environmental).Optional();
            Map(m => m.BusinessContinuity).Optional();
            Map(m => m.Reputation).Optional();
            Map(m => m.LossImpactOnTheCompany).Optional();
            Map(m => m.HighestCriticality).Optional();
        }
    }
}
