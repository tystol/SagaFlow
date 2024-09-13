using System.Text.Json;
using System.Text.Json.Serialization;
using SagaFlow.History;

namespace SagaFlow.AspNetCore.Formatters;

public class SagaFlowCommandIdJsonConverter : JsonConverter<SagaFlowCommandId>
{
    public override SagaFlowCommandId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = JsonSerializer.Deserialize<Guid?>(ref reader, options);

        if (guid == null) return null;

        return new SagaFlowCommandId(guid.Value);
    }

    public override void Write(Utf8JsonWriter writer, SagaFlowCommandId? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value.Value, options);
    }
} 