using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaFlow.Authentications;
using SagaFlow.Schema;
using SagaFlow.Time;

namespace SagaFlow.History;

public class InMemorySagaFlowActivityStore : ISagaFlowActivityReporter, ISagaFlowActivityStore
{
    private readonly SagaFlowModule _sagaFlowModule;
    private readonly IUsernameProvider _identityProvider;
    private readonly ISagaFlowTime _sagaFlowTime;
    private readonly IEnumerable<ISagaFlowEventHandler> _eventHandlers;
    private readonly IHumanReadableCommandNameResolver _commandNameResolver;

    private static readonly ConcurrentDictionary<SagaFlowCommandId, SagaFlowCommandState> Commands = new();
    private static readonly ConcurrentDictionary<SagaFlowMessageId, SagaFlowMessageState> Messages = new();
    private static readonly ConcurrentDictionary<SagaFlowSagaId, SagaFlowSagaState> Sagas = new();

    public InMemorySagaFlowActivityStore(
        SagaFlowModule sagaFlowModule,
        IUsernameProvider identityProvider,
        ISagaFlowTime sagaFlowTime,
        IEnumerable<ISagaFlowEventHandler> eventHandlers,
        IHumanReadableCommandNameResolver commandNameResolver)
    {
        _sagaFlowModule = sagaFlowModule;
        _identityProvider = identityProvider;
        _sagaFlowTime = sagaFlowTime;
        _eventHandlers = eventHandlers;
        _commandNameResolver = commandNameResolver;
    }
    
    public Task RecordCommandInitiated(SagaFlowCommandId commandId, object commandMessage)
    {
        var commandType = commandMessage.GetType();
        var commandDefinition = _sagaFlowModule.Commands.FirstOrDefault(c => c.CommandType == commandType);
        if (commandDefinition == null) throw new ArgumentException($"No SagaFlow command definition for type {commandType}.");
        
        var success = Commands.TryAdd(commandId, new SagaFlowCommandState
        {
            CommandId = commandId,
            CommandDefinition = commandDefinition,
            CommandType = commandType,
            CommandBody = commandMessage,
            Summary = _commandNameResolver.ResolveCommandName(commandMessage),
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
                    StartTime = _sagaFlowTime.Now,
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
                handlerState.CompletionTime = _sagaFlowTime.Now;
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
            StartTime = _sagaFlowTime.Now,
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

        if (sagaFinished)
            sagaState.CompletionTime = _sagaFlowTime.Now;
        
        return Task.CompletedTask;
    }

    public async Task RecordCommandProgress(SagaFlowCommandId commandId, double progress)
    {
        if (!Commands.TryGetValue(commandId, out var command))
            throw new ArgumentException($"A command with id {commandId} does not exists.");

        command.Progress = progress;
        
        // TODO: Refactor this - InMemoryActivityStore shouldn't be responsible for raising core SagaFlow events.
        foreach (var handler in _eventHandlers)
        {
            await handler.ProgressChanged(commandId, progress);
        }
    }

    public Task<PagedResult<SagaFlowCommandStatus>> GetCommandHistory<T>(int pageIndex, int pageSize)
    {
        var query = Commands.Values
            .AsEnumerable()
            .Where(command => command.CommandType == typeof(T));
        
        var total = query.Count();
        var items = query
            .OrderByDescending(command => command.StartTime)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .Select(cs => new SagaFlowCommandStatus
            {
                SagaFlowCommandId = cs.CommandId,
                CommandName = cs.CommandDefinition.Name,
                CommandType = cs.CommandType.ToString(),
                Command = cs.CommandBody,
                Name = cs.Summary,
                InitiatingUser = cs.InitiatingUser,
                StartDateTime = cs.StartTime.UtcDateTime,
                FinishDateTime = cs.GetCompletionTime()?.UtcDateTime,
                Status = cs.GetStatus(),
                Progress = cs.Progress,
                Attempt = 1, // TODO: Implement attempt tracking.
                LastError = cs.GetErroredHandlerExceptions()?.Message,
                StackTrace = cs.GetErroredHandlerExceptions()?.StackTrace,
                Handlers = cs.HandlerStates
                    .Select(h => new CommandHandlerStatusSummary(
                        // TODO: metadata to give nice names to handlers
                        h.HandlerType.Name,
                        h.Status,
                        h.StartTime.UtcDateTime
                    ))
                    .ToList(),
                RelatedSagas = cs.SagaStates.Values
                    .Select(ss => new SagaStatusSummary
                    (
                        ss.SagaType.Name, // TODO: Provide attribute/customized saga naming
                        ss.SagaId,
                        ss.Status,
                        ss.StartTime.UtcDateTime
                    ))
                    .OrderBy(ss => ss.StartTime)
                    .ToList()
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
    private double? progress;
    
    public SagaFlowCommandId CommandId { get; init; }
    public Schema.Command CommandDefinition { get; init; }
    public Type CommandType { get; init; }
    public object CommandBody { get; init; }
    public string Summary { get; init; }
    public string? InitiatingUser { get; init; }
    public DateTimeOffset StartTime { get; init; }

    public double Progress
    {
        get
        {
            // TODO: handle scenario that explicit progress hasn't been set to 100, but all command handlers have finished?
            if (progress != null) return progress.Value;
            if (HandlerStates.Count == 0) return 0;
            // TODO: should progress be 0 - 1, or 0 - 100?
            return HandlerStates.Count(h => h.CompletionTime != null) / (double) HandlerStates.Count * 100d;
        }
        set => progress = value;
    }
    
    public SagaFlowMessageId MessageId { get; set; }

    public List<SagaFlowHandlerState> HandlerStates { get; } = new();

    public Dictionary<SagaFlowSagaId,SagaFlowSagaState> SagaStates { get; } = new();

    public CommandStatus GetStatus()
    {
        if (HandlerStates.All(h => h.CompletionTime != null))
        {
            return HandlerStates.Any(h => h.Error != null) ? CommandStatus.Errored : CommandStatus.Completed;
        }

        return HandlerStates.Count > 0 ? CommandStatus.Processing : CommandStatus.Sent;
    }

    public DateTimeOffset? GetCompletionTime()
    {
        // TODO: should an associated saga need to finish to deem a command completed? Or is the fact that the command started a saga and that saga step completed enough?
        if (HandlerStates.All(h => h.CompletionTime != null) && SagaStates.Values.All(s => s.CompletionTime != null))
        {
            var handlerMax = HandlerStates.Count > 0 ? HandlerStates.Max(h => h.CompletionTime) : null;
            var sagaMax = SagaStates.Count > 0 ? SagaStates.Values.Max(h => h.CompletionTime) : null;
            return sagaMax == null || handlerMax > sagaMax ? handlerMax : sagaMax;
        }

        return null;
    }

    public Exception? GetErroredHandlerExceptions()
    {
        var errors = HandlerStates
            .Where(h => h.Error != null)
            .Select(h => h.Error!)
            .ToList();

        return errors.Count switch
        {
            0 => null,
            1 => errors[0],
            _ => new AggregateException(errors)
        };
    }
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
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset? CompletionTime { get; set; }
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
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset? CompletionTime { get; set; }
    public List<Exception> Errors { get; } = [];
    public List<SagaFlowMessageState> Messages { get; } = [];
}