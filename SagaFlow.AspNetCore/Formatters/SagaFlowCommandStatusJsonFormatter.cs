using Microsoft.AspNetCore.Mvc.Formatters;

namespace SagaFlow.AspNetCore.Formatters;

public class SagaFlowCommandStatusJsonFormatter : SystemTextJsonOutputFormatter
{
    public SagaFlowCommandStatusJsonFormatter() : base(SagaFlowCommandStatusJsonSerializerOptions.JsonSerializerOptions)
    {
    }
}