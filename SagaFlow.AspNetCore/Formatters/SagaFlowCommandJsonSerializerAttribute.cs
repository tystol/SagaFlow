using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SagaFlow.AspNetCore.Formatters;

/// <summary>
/// Attribute to append to API controllers that should follow the Json serializer settings
/// required to correctly understand SagaFlow command statuses.  Should be applied to any
/// API controller endpoint which the SagaFlow.UI svelte components directly talk to.
/// 
/// This is so settings such as property casing and enum treatments are consistent.
/// </summary>
public class SagaFlowCommandJsonSerializerAttribute: Attribute, IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            objectResult.Formatters.Clear();
            objectResult.Formatters.Add(new SagaFlowCommandStatusJsonFormatter());
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // not required
    }
}