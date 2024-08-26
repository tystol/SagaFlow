using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SagaFlow.AspNetCore.Formatters;

/// <summary>
/// This is the Json Serializer options used by the SagaFlow status webapi, this is so we can control the JSON
/// output from any SagaFlow Command Status api call or anything related to SagaFlow command statuses that
/// required serialization to JSON.
///
/// This is so settings such as casing, handling of enums are consistent to avoid issues with SagaFlow UI
/// misinterpreting 
/// </summary>
public static class SagaFlowCommandStatusJsonSerializerOptions
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new SagaFlowIdJsonConverterFactory(),
        },
        
        // need to specify default for net8 issue: https://github.com/dotnet/aspnetcore/issues/55692
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        
        #if DEBUG
        WriteIndented = true
        #endif
    };
}