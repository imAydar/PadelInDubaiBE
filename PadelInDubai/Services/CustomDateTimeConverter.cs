using System.Globalization;
using Newtonsoft.Json;

namespace PadelInDubai.Services
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string Format = "dd.MM.yyyy H:mm:ss";

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            throw new FormatException($"Could not parse DateTime: {value}");
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            var dt = (DateTime)value;
            writer.WriteValue(dt.ToString(Format));
        }
    }
}
