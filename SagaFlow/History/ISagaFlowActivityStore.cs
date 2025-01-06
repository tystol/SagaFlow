using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaFlow.History;

public interface ISagaFlowActivityStore
{
    /// <summary>
    /// Records a command message being sent via the SagaFlow API. 
    /// </summary>
    /// <remarks>
    /// This is separate from every message sent over the message bus. eg. a command may initiate a handler or saga,
    /// that then triggers multiple other messages to be sent. Only this first command message sent via the SagaFlow
    /// API is treated as an actual command. All other messages are just correlated messages that are linked back
    /// to this initiating command.
    /// </remarks>
    Task RecordCommandInitiated(SagaFlowCommandId commandId, object commandMessage);

    Task RecordMessageSent(SagaFlowMessageId messageId, SagaFlowCommandId? commandId, string? correlationId, object message);

    Task RecordHandlerStarted(SagaFlowMessageId messageId, string? correlationId, object message, object handler);

    Task RecordHandlerFinished(SagaFlowMessageId messageId, string? correlationId, object message, object handler, Exception? error);

    Task RecordSagaStepStarted(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga);

    Task RecordSagaStepFinished(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga, Exception? error, bool sagaFinished);
}

public class InMemorySagaFlowActivityStore : ISagaFlowActivityStore
{
    private static readonly ConcurrentDictionary<SagaFlowCommandId, SagaFlowCommandState> Commands = new();
    private static readonly ConcurrentDictionary<SagaFlowMessageId, SagaFlowMessageState> Messages = new();

    
    public Task RecordCommandInitiated(SagaFlowCommandId commandId, object commandMessage)
    {
        var success = Commands.TryAdd(commandId, new SagaFlowCommandState
        {
            CommandId = commandId,
            CommandBody = commandMessage,
        });

        if (!success)
        {
            // TODO: Throw if command already exists?
            throw new ArgumentException($"A command with id {commandId} already exists.");
        }
        
        return Task.CompletedTask;
    }

    public Task RecordMessageSent(SagaFlowMessageId messageId, SagaFlowCommandId? commandId, string? correlationId, object message)
    {
        Messages.TryAdd(messageId, new SagaFlowMessageState
        {
            MessageId = messageId,
            CommandId = commandId,
            CorrelationId = correlationId,
            MessageBody = message,
        });
        
        return Task.CompletedTask;
    }

    public Task RecordHandlerStarted(SagaFlowMessageId messageId, string? correlationId, object message, object handler)
    {
        var messageExists = Messages.TryGetValue(messageId, out var messageState);
        if (!messageExists)
            throw new ArgumentException($"Message with id {messageId} does not exist.");

        if (messageState?.CommandId != null)
        {
            var commandExists = Commands.TryGetValue(messageState.CommandId, out var commandState);
            if (commandExists)
            {
                var handlerState = new SagaFlowHandlerState
                {
                    InitiatingMessageId = messageId,
                    HandlerType = handler.GetType(),
                    Status = HandlerStatus.Running,
                };
                commandState!.HandlerStates.Add(handlerState);
            }
        }
        
        return Task.CompletedTask;
    }

    public Task RecordHandlerFinished(SagaFlowMessageId messageId, string? correlationId, object message, object handler, Exception? error)
    {
        var messageExists = Messages.TryGetValue(messageId, out var messageState);
        if (!messageExists)
            throw new ArgumentException($"Message with id {messageId} does not exist.");

        if (messageState?.CommandId != null)
        {
            var commandExists = Commands.TryGetValue(messageState.CommandId, out var commandState);
            if (commandExists)
            {
                var handlerState = commandState!.HandlerStates.Find(h => h.HandlerType == handler.GetType());
                if (handlerState == null)
                    throw new ArgumentException($"Could not find handler state for handler {handler.GetType()}");

                // TODO: track retries and only progress to Failed once retry limit reached.
                handlerState.Status = error == null ? HandlerStatus.Complete : HandlerStatus.Failed;
                handlerState.Error = error;
            }
        }
        
        return Task.CompletedTask;
    }

    public Task RecordSagaStepStarted(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga)
    {
        return Task.CompletedTask;
    }

    public Task RecordSagaStepFinished(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga, Exception? error, bool sagaFinished)
    {
        return Task.CompletedTask;
    }
}

public class SagaFlowCommandState
{
    public SagaFlowCommandId CommandId { get; init; }
    public object CommandBody { get; init; }
    
    public SagaFlowMessageId MessageId { get; set; }

    public List<SagaFlowHandlerState> HandlerStates { get; } = new();

    public List<SagaFlowSagaState> SagaStates { get; } = new();
}

public class SagaFlowMessageState
{
    public SagaFlowMessageId MessageId { get; init; }
    public SagaFlowCommandId? CommandId { get; init; }
    public string? CorrelationId { get; init; }
    public object MessageBody { get; init; }
}

public enum HandlerStatus
{
    Running,
    RunningWithErrors,
    Complete,
    Failed,
}

public class SagaFlowHandlerState
{
    public SagaFlowMessageId InitiatingMessageId { get; init; }
    public Type HandlerType { get; init; }
    public HandlerStatus Status { get; set; }
    public Exception? Error { get; set; }
}

public class SagaFlowSagaState
{
    public SagaFlowSagaId SagaId { get; init; }
}