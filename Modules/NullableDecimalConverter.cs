using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableDecimalConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text) || text == "0")
            {
                return null;  // Return null for empty, whitespace, or zero values
            }

            // Log the raw text before processing for debugging
            Console.WriteLine($"Row {row.Context.Parser.Row}: Raw decimal value: {text}");

            // Remove spaces, commas, and currency symbols
            text = text.Replace(",", "").Replace("$", "").Trim();

            // Try parsing the cleaned-up text as a decimal
            if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                Console.WriteLine($"Row {row.Context.Parser.Row}: Successfully parsed value: {decimalValue}");
                return decimalValue;
            }

            // Log failure to parse the value
            Console.WriteLine($"Row {row.Context.Parser.Row}: Failed to parse value: {text}. Raw data: {row.Context.Parser.RawRecord}");

            return null;  // Return null if parsing fails
        }
    }
}
