using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace SagaFlow.AspNetCore.Rebus;


[StepDocumentation("Provides access to the current aspnet request scope for downstream outgoing message steps.")]
public class PerHttpRequestScopeInjector : IOutgoingStep
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerHttpRequestScopeInjector(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var requestScope = _httpContextAccessor.HttpContext?.RequestServices;
        if (requestScope != null)
            context.Save(requestScope);

        return next();
    }
}