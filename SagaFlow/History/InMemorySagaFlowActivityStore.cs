using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaFlow.Authentications;
using SagaFlow.Time;

namespace SagaFlow.History;

public class InMemorySagaFlowActivityStore : ISagaFlowActivityReporter, ISagaFlowActivityStore
{
    private readonly IUsernameProvider _identityProvider;
    private readonly ISagaFlowTime _sagaFlowTime;
    private static readonly ConcurrentDictionary<SagaFlowCommandId, SagaFlowCommandState> Commands = new();
    private static readonly ConcurrentDictionary<SagaFlowMessageId, SagaFlowMessageState> Messages = new();
    private static readonly ConcurrentDictionary<SagaFlowSagaId, SagaFlowSagaState> Sagas = new();

    public InMemorySagaFlowActivityStore(IUsernameProvider identityProvider, ISagaFlowTime sagaFlowTime)
    {
        _identityProvider = identityProvider;
        _sagaFlowTime = sagaFlowTime;
    }
    
    public Task RecordCommandInitiated(SagaFlowCommandId commandId, object commandMessage)
    {
        var success = Commands.TryAdd(commandId, new SagaFlowCommandState
        {
            CommandId = commandId,
            CommandType = commandMessage.GetType(),
            CommandBody = commandMessage,
            InitiatingUser = _identityProvider.CurrentUsername,
            StartTime = _sagaFlowTime.Now,
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
        var messageExists = Messages.TryGetValue(messageId, out var messageState);
        if (!messageExists)
            throw new ArgumentException($"Message with id {messageId} does not exist.");
        
        var sagaState = Sagas.GetOrAdd(sagaId, id => new SagaFlowSagaState
        {
            SagaId = id,
            SagaType = saga.GetType(),
        });

        sagaState.Status = sagaState.Status == SagaStatus.RunningWithErrors ? SagaStatus.RunningWithErrors : SagaStatus.Running;
        sagaState.Messages.Add(messageState!);

        if (messageState?.CommandId != null)
        {
            var commandExists = Commands.TryGetValue(messageState.CommandId, out var commandState);
            if (commandExists)
            {
                commandState!.SagaStates.TryAdd(sagaId, sagaState);
            }
        }
        
        return Task.CompletedTask;
    }

    public Task RecordSagaStepFinished(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga, Exception? error, bool sagaFinished)
    {
        var messageExists = Messages.TryGetValue(messageId, out var messageState);
        if (!messageExists)
            throw new ArgumentException($"Message with id {messageId} does not exist.");
        
        var sagaState = Sagas.GetOrAdd(sagaId, id => new SagaFlowSagaState
        {
            SagaId = id,
            SagaType = saga.GetType(),
        });

        if (error != null)
            sagaState.Errors.Add(error);

        sagaState.Status = sagaFinished ? 
            sagaState.Errors.Count == 0 ? SagaStatus.Complete : SagaStatus.CompleteWithErrors : 
            sagaState.Errors.Count == 0 ? SagaStatus.Running : SagaStatus.RunningWithErrors;
        
        return Task.CompletedTask;
    }
    
    public Task<PagedResult<SagaFlowCommandStatus>> GetCommandHistory<T>(int pageIndex, int pageSize)
    {
        var query = Commands.Values
            .AsQueryable()
            .Where(command => command.CommandType == typeof(T));
        
        var total = query.Count();
        var items = query
            .OrderByDescending(command => command.StartTime)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .Select( s => new SagaFlowCommandStatus
            {
                SagaFlowCommandId = s.CommandId,
                
            });

        return Task.FromResult(new PagedResult<SagaFlowCommandStatus>(
            items,
            pageIndex,
            pageSize,
            total));
    }
}

public class SagaFlowCommandState
{
    public SagaFlowCommandId CommandId { get; init; }
    public Type CommandType { get; init; }
    public object CommandBody { get; init; }
    public string? InitiatingUser { get; init; }
    public DateTimeOffset StartTime { get; init; }

    
    public SagaFlowMessageId MessageId { get; set; }

    public List<SagaFlowHandlerState> HandlerStates { get; } = new();

    public Dictionary<SagaFlowSagaId,SagaFlowSagaState> SagaStates { get; } = new();
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

public enum SagaStatus
{
    Running,
    RunningWithErrors,
    Complete,
    CompleteWithErrors,
}

public class SagaFlowSagaState
{
    public SagaFlowSagaId SagaId { get; init; }
    public Type SagaType { get; init; }
    public SagaStatus Status { get; set; }
    public List<Exception> Errors { get; } = [];
    public List<SagaFlowMessageState> Messages { get; } = [];
}