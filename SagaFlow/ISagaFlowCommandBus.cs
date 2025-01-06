using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus.Bus;
using SagaFlow.History;

namespace SagaFlow;

public interface ISagaFlowCommandBus
{
    Task Send(object command);
}

public class RebusCommandBus : ISagaFlowCommandBus
{
    private readonly ISagaFlowActivityStore activityStore;
    private readonly IBus rebus;

    public RebusCommandBus(ISagaFlowActivityStore activityStore, IBus rebus)
    {
        this.activityStore = activityStore;
        this.rebus = rebus;
    }
    public async Task Send(object command)
    {
        var commandId = new SagaFlowCommandId(Guid.NewGuid());
        var headers = new Dictionary<string, string> {{SagaFlowRebusEvents.SagaFlowCommandId, commandId}};
        // TODO: mark command as scheduled/recurring
        await activityStore.RecordCommandInitiated(commandId, command);
        await rebus.Send(command, headers);
        // TODO: Offer config over whether to send commands as a direct message or publish as event?
        // await bus.Publish(value);
    }
}