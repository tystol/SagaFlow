using System;

namespace SagaFlow.Time;

public interface ISagaFlowTime
{
    /// <summary>
    /// Gets the current time
    /// </summary>
    DateTimeOffset Now { get; }
}