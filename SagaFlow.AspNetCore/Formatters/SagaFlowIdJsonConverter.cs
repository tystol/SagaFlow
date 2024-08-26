using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SagaFlow.History;

namespace SagaFlow.AspNetCore.Formatters;

public class SagaFlowIdJsonConverter<T> : JsonConverter<T> where T : class, ISagaFlowId
{
    private readonly JsonConverter<Guid?> guidConverter;
    public SagaFlowIdJsonConverter(JsonSerializerOptions options)
    {
        // For performance, use the existing converter.
        guidConverter = (JsonConverter<Guid?>)options.GetConverter(typeof(Guid?));
    }
    
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = guidConverter.Read(ref reader, typeof(Guid?), options)!;

        if (guid == null) return null;

        return (T) Activator.CreateInstance(typeToConvert, guid.Value)!;
    }

    public override void Write(Utf8JsonWriter writer, T? id, JsonSerializerOptions options)
    {
        if (id is null)
            writer.WriteNullValue();
        else
            guidConverter.Write(writer, id.Value, options);
    }
}

public class SagaFlowIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(ISagaFlowId));
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(SagaFlowIdJsonConverter<>).MakeGenericType(type),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null)!;

        return converter;
    }
}