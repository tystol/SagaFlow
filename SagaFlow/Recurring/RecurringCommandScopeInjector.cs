using System;
using System.Threading.Tasks;
using Rebus.Pipeline;

namespace SagaFlow.Recurring;

[StepDocumentation("Provides access to the recurring command scheduler scope for downstream outgoing message steps.")]
public class RecurringCommandScopeInjector : IOutgoingStep
{
    private readonly IRecurringCommandScopeAccessor scopeAccessor;

    public RecurringCommandScopeInjector(IRecurringCommandScopeAccessor scopeAccessor)
    {
        this.scopeAccessor = scopeAccessor;
    }
    
    public Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var schedulerScope = scopeAccessor.Scope;
        if (schedulerScope != null)
            context.Save(schedulerScope);

        return next();
    }
}