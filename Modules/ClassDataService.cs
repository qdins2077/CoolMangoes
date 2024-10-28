using CoolMangoes.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CoolMangoes.Modules
{
    public class ClassDataService
    {
        public List<ClassData> LoadClassData(string filePath)
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
                csvReader.Context.TypeConverterCache.AddConverter<float?>(new NullableFloatConverter());

                csvReader.Context.RegisterClassMap<ClassDataMap>();

                try
                {
                    var classDataList = csvReader.GetRecords<ClassData>().ToList();
                    return classDataList;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error reading class data: {ex.Message}", ex);
                }
            }
        }
    }
}
