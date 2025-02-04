using System;

namespace SagaFlow.History;

public interface ISagaFlowId
{
    Guid Value { get; }
}

public record SagaFlowMessageId(Guid Value) : ISagaFlowId
{
    public SagaFlowMessageId() : this(Guid.NewGuid())
    { }

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public static implicit operator string(SagaFlowMessageId id) => id.ToString();
    public static implicit operator SagaFlowMessageId(string value) => new(Guid.Parse(value));
    public static implicit operator SagaFlowMessageId(Guid value) => new(value);
}

public record SagaFlowCommandId(Guid Value) : ISagaFlowId
{
    public SagaFlowCommandId() : this(Guid.NewGuid())
    { }

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public static implicit operator string(SagaFlowCommandId id) => id.ToString();
    public static implicit operator SagaFlowCommandId(string value) => new(Guid.Parse(value));
    public static implicit operator SagaFlowCommandId(Guid value) => new(value);
}

public record SagaFlowSagaId(Guid Value) : ISagaFlowId
{
    public SagaFlowSagaId() : this(Guid.NewGuid())
    { }

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public static implicit operator string(SagaFlowSagaId id) => id.ToString();
    public static implicit operator SagaFlowSagaId(string value) => new(Guid.Parse(value));
    public static implicit operator SagaFlowSagaId(Guid value) => new(value);
}