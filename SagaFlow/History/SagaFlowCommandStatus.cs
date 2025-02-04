using System;
using System.Collections.Generic;

namespace SagaFlow.History;

public enum CommandStatus
{
    Sent,
    Processing,
    Completed,
    Errored
}

public class SagaFlowCommandStatus
{
    public SagaFlowCommandId SagaFlowCommandId { get; init; }

    public CommandStatus Status { get; set; } = CommandStatus.Sent;
    
    public string Name { get; init; }
    
    public string CommandName { get; init; }
    
    public string CommandType { get; init; }
    public object Command { get; init; }
    
    public string? InitiatingUser { get; init; }
    
    public DateTime StartDateTime { get; init; }
    
    public DateTime? FinishDateTime { get; set; }

    public int Attempt { get; set; }
    
    public double Progress { get; set; }

    public string? LastError { get; set; }
    public string? StackTrace { get; set; }
    
    public List<CommandHandlerStatusSummary> Handlers { get; init; }
    public List<SagaStatusSummary> RelatedSagas { get; init; }
}

public record CommandHandlerStatusSummary(string Name, HandlerStatus Status, DateTime StartTime);

public record SagaStatusSummary(string Name, SagaFlowSagaId SagaId, SagaStatus Status, DateTime StartTime);


public record PagedResult<TItem>(
    IEnumerable<TItem> Page,
    int PageIndex,
    int PageSize,
    int Total);