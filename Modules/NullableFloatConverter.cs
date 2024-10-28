using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace CoolMangoes.Modules
{
    public class NullableFloatConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = text.Replace(",", "").Replace("$", "").Trim();

            if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                return floatValue;

            return null; // Or throw an exception if necessary
        }
    }
}
