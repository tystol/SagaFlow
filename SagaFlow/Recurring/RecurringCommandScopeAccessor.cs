using System;

namespace SagaFlow.Recurring;

public interface IRecurringCommandScopeAccessor
{
    public IServiceProvider? Scope { get; }
}

