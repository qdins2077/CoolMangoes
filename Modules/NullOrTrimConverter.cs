// public class NullOrTrimConverter : DefaultTypeConverter
// {
//     public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
//     {
//         // Trim the value
//         var trimmed = text?.Trim();

//         // If it's null or empty (after trimming), return null
//         return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
//     }
//}
