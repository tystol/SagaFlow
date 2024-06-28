using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Pipeline;

namespace SagaFlow.Status;

public interface ISagaFlowCommandContext
{
    /// <summary>
    /// Updates the progress of a command with a value between 0 and 100.  Where 0 is at the very beginning of the
    /// command, 100 is at the completion of the command.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    Task UpdateProgress(double progress);
}

internal class SagaFlowRebusCommandContext : ISagaFlowCommandContext
{
    private readonly ISagaFlowCommandStatusService _sagaFlowCommandStatusService;
    private readonly IEnumerable<ISagaFlowCommandStateChangedHandler> _updateHandlers;

    public SagaFlowRebusCommandContext(
        ISagaFlowCommandStatusService sagaFlowCommandStatusService, 
        IEnumerable<ISagaFlowCommandStateChangedHandler> updateHandlers)
    {
        _sagaFlowCommandStatusService = sagaFlowCommandStatusService;
        _updateHandlers = updateHandlers;
    }
    
    public async Task UpdateProgress(double progress)
    {
        if (progress < 0 || progress > 100) throw new ArgumentException("Progress must be a value between 0 and 100");
        
        var updatedSagaFlowCommand = await _sagaFlowCommandStatusService.UpdateProgress(
            MessageContext.Current.Headers[SagaFlowRebusEvents.SagaFlowCommandId],
            progress);

        await PublishSagaFlowCommandStateChanged(updatedSagaFlowCommand);
    }

    private async Task PublishSagaFlowCommandStateChanged(SagaFlowCommandStatus updatedSagaFlowCommandStatus)
    {
        await Task.WhenAll(
            _updateHandlers.Select(
                handler => handler.HandleSagaFlowCommandStatusUpdate(updatedSagaFlowCommandStatus)
            )
        );
    }
}

