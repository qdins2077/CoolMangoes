using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System;

namespace CoolMangoes.Modules
{
    using System;
using System.IO;

public class AssetDataService
{
    
    public List<Asset> LoadAssetData(string filePath)
    {
        var assetDataList = new List<Asset>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,  // Handle missing fields
        };

        using (var reader = new StreamReader(filePath))
        using (var csvReader = new CsvReader(reader, config))
        {
            csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new NullableDateTimeConverter());
            csvReader.Context.TypeConverterCache.AddConverter<decimal?>(new NullableDecimalConverter());

            // Register AssetMap
            csvReader.Context.RegisterClassMap<AssetMap>();

            int totalRows = 0;
            int successfullyParsed = 0;
            int skippedRows = 0;

            try
            {
                while (csvReader.Read())
                {
                    totalRows++;
                    try
                    {
                        var record = csvReader.GetRecord<Asset>();

                        // Log the PurchaseCost for each row before adding
                        Console.WriteLine($"Row {totalRows}: PurchaseCost is {record.PurchaseCost}");

                        // Ensure required fields are present
                        if (string.IsNullOrWhiteSpace(record.Asset_ID) || string.IsNullOrWhiteSpace(record.AssetDescription))
                        {
                            Console.WriteLine($"Row {totalRows} skipped: Missing Asset_ID or AssetDescription.");
                            skippedRows++;
                            continue;
                        }

                        successfullyParsed++;
                        assetDataList.Add(record);
                    }
                    catch (Exception parseEx)
                    {
                        skippedRows++;
                        // Log the failed row details
                        Console.WriteLine($"Failed to parse row {totalRows}: {parseEx.Message}. Raw row data: {csvReader.Context.Parser.RawRecord}");
                    }
                }

                Console.WriteLine($"Total rows read: {totalRows}");
                Console.WriteLine($"Total records successfully parsed: {successfullyParsed}");
                Console.WriteLine($"Total rows skipped: {skippedRows}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        return assetDataList;
    }
}




    public class AssetMap : ClassMap<Asset>
    {
        public AssetMap()
        {
            // Explicitly map fields and use custom converter for nullable DateTimes and Decimals
            Map(m => m.AcqDate).TypeConverter<NullableDateTimeConverter>();
            Map(m => m.ObservationDate).TypeConverter<NullableDateTimeConverter>();
            Map(m => m.PlannedStartDate).TypeConverter<NullableDateTimeConverter>();
            Map(m => m.PlannedEndDate).TypeConverter<NullableDateTimeConverter>();
            Map(m => m.PurchaseCost).TypeConverter<NullableFloatConverter>(); // Apply NullableDecimalConverter

            // Map other fields normally
            Map(m => m.Location1);
            Map(m => m.Location2);
            Map(m => m.Location3);
            Map(m => m.Location4);
            Map(m => m.Asset_ID);
            Map(m => m.Parent_ID);
            Map(m => m.AssetDescription);
            Map(m => m.AssetHierarchy);
            Map(m => m.HierarchyL1);
            Map(m => m.HierarchyL2);
            Map(m => m.HierarchyL3);
            Map(m => m.HierarchyL4);
            Map(m => m.HierarchyCode);
            Map(m => m.Manufacturer);
            Map(m => m.ModelNumber);
            Map(m => m.ManufSerialNo);
            Map(m => m.ConditionRating);
            Map(m => m.CurrentUsage);
            Map(m => m.OperatingEnvironment);
            Map(m => m.MaintenanceStrategyCode);
            Map(m => m.MaintenanceType);
            Map(m => m.Statutory);
            Map(m => m.Criticality);
        }
    }
}
