using Newtonsoft.Json;
using System.Globalization;

namespace PadelInDubai.Services
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] SupportedFormats = new[]
        {
        "dd.MM.yyyy H:mm:ss",     // 15.03.2025 0:00:30
        "yyyy-MM-dd HH:mm:ss",    // 2025-03-15 17:00:00
        "yyyy-MM-ddTHH:mm:ss",    // 2025-03-15T17:00:00
        "yyyy-MM-ddTHH:mm:ssK",   // 2025-03-15T17:00:00+04:00
        "yyyy-MM-ddTHH:mm:sszzz"  // 2025-03-15T17:00:00+04:00
    };

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (DateTime.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var result))
                return result;

            throw new FormatException($"Could not parse DateTime: {value}");
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            var dt = value;
            writer.WriteValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
