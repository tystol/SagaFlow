using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SagaFlow.History;

public record SagaFlowMessageId(Guid Value)
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

public record SagaFlowCommandId(Guid Value)
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

public record SagaFlowSagaId(Guid Value)
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

public enum CommandStatus
{
    Started,
    Processing,
    Completed,
    Errored
}

public class SagaFlowCommandStatus
{
    public SagaFlowCommandId SagaFlowCommandId { get; init; }

    public CommandStatus Status { get; set; } = CommandStatus.Started;
    
    public string Name { get; init; }
    
    public string CommandName { get; init; }
    
    public string CommandType { get; init; }
    public object Command { get; init; }
    
    public string CommandArgs { get; init; }
    
    public ReadOnlyDictionary<string, string> HumanReadableCommandPropertyValues { get; init; }
    
    public string InitiatingUser { get; init; }
    
    public DateTime StartDateTime { get; init; }
    
    public DateTime? FinishDateTime { get; set; }

    public int Attempt { get; set; }
    
    public double Progress { get; set; }

    public string? LastError { get; set; }
    public string? StackTrace { get; set; }
}


public record PagedResult<TItem>(
    IEnumerable<TItem> Page,
    int PageIndex,
    int PageSize,
    int Total);