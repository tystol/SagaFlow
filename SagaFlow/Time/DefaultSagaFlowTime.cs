using System;

namespace SagaFlow.Time;

public class DefaultSagaFlowTime : ISagaFlowTime
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}