using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

namespace CoolMangoes.Modules
{
    public class ClassDataService
    {
        private List<ClassData> classDataList = new List<ClassData>();

        public List<ClassData> LoadClassData(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,  // Handle missing fields
            };

            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new NullableDateTimeConverter());

                // Register ClassDataMap
                csvReader.Context.RegisterClassMap<ClassDataMap>();

                classDataList = csvReader.GetRecords<ClassData>().ToList();
            }

            return classDataList;
        }

        // Expose the current ClassData list to other services
        public List<ClassData> GetClassDataList()
        {
            return classDataList;
        }
    }

    // Define ClassDataMap class to map fields between CSV columns and ClassData properties
    public class ClassDataMap : ClassMap<ClassData>
    {
        public ClassDataMap()
        {
            // Map all necessary fields, allowing nulls where necessary
            Map(m => m.AssetHierarchy);
            Map(m => m.HierarchyCode);
            Map(m => m.AssetType);
            Map(m => m.MaintenanceType);
            Map(m => m.Statutory).Optional(); // Allow null or empty
            Map(m => m.EstimatedLife).TypeConverter<NullableFloatConverter>(); // Allow null or empty
            Map(m => m.RefurbishmentFrequency).TypeConverter<NullableFloatConverter>(); // Allow null or empty
            Map(m => m.RefurbishmentCostAsProportionOfReplacementCost).TypeConverter<NullableFloatConverter>(); // Allow null or empty
            Map(m => m.MinCost).TypeConverter<NullableFloatConverter>(); // Apply to MinCost
            Map(m => m.MaxCost).TypeConverter<NullableFloatConverter>(); // Apply to MaxCost
            Map(m => m.AvgReplacementCost).TypeConverter<NullableFloatConverter>(); // Apply to AvgReplacementCost
        }
    }
}
