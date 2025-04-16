using Newtonsoft.Json;
using System.Globalization;

namespace PadelInDubai.Services
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] SupportedFormats = new[]
        {
            "dd.MM.yyyy H:mm:ss",         // 15.03.2025 0:00:30
            "yyyy-MM-dd HH:mm:ss",        // 2025-03-15 17:00:00
            "yyyy-MM-ddTHH:mm:ss",        // 2025-03-15T17:00:00
            "yyyy-MM-ddTHH:mm:ssK",       // 2025-03-15T17:00:00+04:00
            "yyyy-MM-ddTHH:mm:sszzz",     // 2025-03-15T17:00:00+04:00
            "M/d/yyyy h:mm:ss tt",        // 4/14/2025 10:05:29 AM
            "M/d/yyyy h:mm:ss\u202Ftt",   // 4/14/2025 10:05:29 AM (narrow no-break space)
            "M/d/yyyy h:mm:ss\u00A0tt"    // 4/14/2025 10:05:29 AM (regular no-break space)
        };

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            // Try all supported formats
            if (DateTime.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var result))
                return result;

            // Fallback: try default parsing
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result))
                return result;

            throw new FormatException($"Could not parse DateTime: {value}");
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
