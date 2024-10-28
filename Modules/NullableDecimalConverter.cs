using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableDecimalConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = text.Replace(",", "").Replace("$", "").Trim();

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                return decimalValue;

            return null; // Or throw an exception if necessary
        }
    }
}
