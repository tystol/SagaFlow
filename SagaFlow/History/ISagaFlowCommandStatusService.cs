using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Rebus.Sagas;

namespace SagaFlow.History;

public interface ISagaFlowCommandStatusService
{
    Task<SagaFlowCommandStatus> UpdateProgress(SagaFlowCommandId sagaFlowCommandId, double progress);
    
    Task<SagaFlowCommandStatus> UpdateErrored(SagaFlowCommandId sagaFlowCommandId, Exception exception);

    Task UpdateSagaStartingHandler(SagaFlowCommandId sagaFlowCommandId, Saga saga);
    Task UpdateSagaFinishedHandler(SagaFlowCommandId sagaFlowCommandId, Saga saga);
    
    Task<object> GetCommandParameters(SagaFlowCommandId sagaFlowCommandId);
}

/// <summary>
/// A simple implementation of an in-memory SagaFlow command status store.  It will store and manage the
/// status of the last 300 command sent to SagaFlow to process.  It will drop the oldest commands and
/// their status when a new command has started and their will be more than 300 statuses.
/// </summary>
internal class SagaFlowCommandStatusService : ISagaFlowCommandStatusService
{
    private readonly ISagaFlowCommandStore _sagaFlowCommandStore;
    private readonly SagaFlowModule _sagaFlowModule;

    public SagaFlowCommandStatusService(
        ISagaFlowCommandStore sagaFlowCommandStore,
        SagaFlowModule sagaFlowModule)
    {
        _sagaFlowCommandStore = sagaFlowCommandStore;
        _sagaFlowModule = sagaFlowModule;
    }

    public async Task<SagaFlowCommandStatus> UpdateProgress(SagaFlowCommandId sagaFlowCommandId, double progress)
    {
        var commandStatus = await _sagaFlowCommandStore.GetCommand(sagaFlowCommandId) ?? throw new InvalidOperationException("Command not found");

        commandStatus.Status = CommandStatus.Processing;
        commandStatus.Progress = progress;

        if (progress >= 100.0)
        {
            commandStatus.Status = CommandStatus.Completed;
            commandStatus.FinishDateTime = DateTime.UtcNow;

            commandStatus.LastError = null;
            commandStatus.StackTrace = null;
        }

        await _sagaFlowCommandStore.AddOrUpdateCommand(commandStatus);
        
        return commandStatus;
    }

    public async Task<SagaFlowCommandStatus> UpdateErrored(SagaFlowCommandId sagaFlowCommandId, Exception exception)
    {
        var commandStatus = await _sagaFlowCommandStore.GetCommand(sagaFlowCommandId) ?? throw new InvalidOperationException("Command not found");

        commandStatus.Status = CommandStatus.Errored;
        commandStatus.FinishDateTime = DateTime.UtcNow;

        commandStatus.LastError = exception.Message;
        commandStatus.StackTrace = exception.StackTrace;
        
        await _sagaFlowCommandStore.AddOrUpdateCommand(commandStatus);
        
        return commandStatus;
    }

    public async Task UpdateSagaStartingHandler(SagaFlowCommandId sagaFlowCommandId, Saga saga)
    {
        var commandStatus = await _sagaFlowCommandStore.GetCommand(sagaFlowCommandId) ?? throw new InvalidOperationException("Command not found");

        
        
        await _sagaFlowCommandStore.AddOrUpdateCommand(commandStatus);
    }

    public async Task UpdateSagaFinishedHandler(SagaFlowCommandId sagaFlowCommandId, Saga saga)
    {
        var commandStatus = await _sagaFlowCommandStore.GetCommand(sagaFlowCommandId) ?? throw new InvalidOperationException("Command not found");
        
        
        await _sagaFlowCommandStore.AddOrUpdateCommand(commandStatus);
    }

    public async Task<object> GetCommandParameters(SagaFlowCommandId sagaFlowCommandId)
    {
        var command = await _sagaFlowCommandStore.GetCommand(sagaFlowCommandId) ?? throw new InvalidOperationException("Command not found");
        var commandDefinition = _sagaFlowModule.Commands.FirstOrDefault(def => def.CommandType.Name == command.CommandType) ?? throw new InvalidOperationException("Expected command to be defined in SagaFlow module");

        return Task.FromResult(JsonSerializer.Deserialize(command.CommandArgs, commandDefinition.CommandType));
    }
}