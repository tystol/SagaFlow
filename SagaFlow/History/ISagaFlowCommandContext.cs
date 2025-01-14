using System;
using System.Threading.Tasks;
using Rebus.Pipeline;

namespace SagaFlow.History;

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
    private readonly ISagaFlowActivityReporter sagaFlowActivityReporter;

    public SagaFlowRebusCommandContext(ISagaFlowActivityReporter sagaFlowActivityReporter)
    {
        this.sagaFlowActivityReporter = sagaFlowActivityReporter;
    }
    
    public Task UpdateProgress(double progress)
    {
        if (progress < 0 || progress > 100) throw new ArgumentException("Progress must be a value between 0 and 100");
        var commandId = MessageContext.Current.Headers[Headers.SagaFlowCommandId];
        return sagaFlowActivityReporter.RecordCommandProgress(commandId, progress);
    }
}

