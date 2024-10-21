using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableDateTimeConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Custom date formats
            string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
            {
                return dateValue;
            }

            // If the conversion fails, return null or handle the error
            return null;
        }
    }
}
