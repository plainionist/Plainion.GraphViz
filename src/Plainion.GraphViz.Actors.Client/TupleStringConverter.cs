using System;
using Newtonsoft.Json;

namespace Plainion.GraphViz.Actors.Client;

public class TupleStringConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Tuple<string, string>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value as string;
        if (value != null)
        {
            // Remove parentheses and split by comma
            var parts = value.Trim('(', ')').Split(new[] { ", " }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return Tuple.Create(parts[0], parts[1]);
            }
        }
        throw new JsonSerializationException($"Cannot deserialize '{value}' to Tuple<string, string>");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var tuple = (Tuple<string, string>)value;
        writer.WriteValue($"({tuple.Item1}, {tuple.Item2})");
    }
}
