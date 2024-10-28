using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableDateTimeConverter : DefaultTypeConverter
    {
        private static readonly string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (DateTime.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                return dateValue;

            return null; // Or throw an exception if necessary
        }
    }
}
