using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableFloatConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text) || text == "0")
            {
                return null;  // Return null for empty, whitespace, or zero values
            }

            // Log the raw text before processing for debugging
            Console.WriteLine($"Row {row.Context.Parser.Row}: Raw float value: {text}");

            // Remove spaces, commas, and currency symbols
            text = text.Replace(",", "").Replace("$", "").Trim();

            // Try parsing the cleaned-up text as a float
            if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float floatValue))
            {
                Console.WriteLine($"Row {row.Context.Parser.Row}: Successfully parsed value: {floatValue}");
                return floatValue;
            }

            // Log failure to parse the value
            Console.WriteLine($"Row {row.Context.Parser.Row}: Failed to parse value: {text}. Raw data: {row.Context.Parser.RawRecord}");

            return null;  // Return null if parsing fails
        }
    }
}
