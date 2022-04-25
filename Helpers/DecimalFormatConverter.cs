using Newtonsoft.Json;

namespace RD2LPowerRankings.Helpers;

public class DecimalFormatConverter : JsonConverter
{
    public override bool CanRead => false;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type type)
    {
        return type == typeof(decimal);
    }

    public override void WriteJson(JsonWriter writer, object? value,
        JsonSerializer serializer)
    {
        writer.WriteRawValue($"{value:0.00}");
    }
}