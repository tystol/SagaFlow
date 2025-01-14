using Microsoft.AspNetCore.SignalR;
using SagaFlow.SignalR.Hubs;
using SagaFlow.History;

namespace SagaFlow.SignalR;

public class PublishSagaFlowEventsToSignalRHub : ISagaFlowEventHandler
{
    private readonly IHubContext<SagaFlowSignalRHub, ISagaFlowSignalRHub> _hub;

    public PublishSagaFlowEventsToSignalRHub(IHubContext<SagaFlowSignalRHub, ISagaFlowSignalRHub> hub)
    {
        _hub = hub;
    }
    
    public async Task HandleSagaFlowCommandStatusUpdate(SagaFlowCommandStatus sagaFlowCommandStatus)
    {
        await _hub.Clients.All.SendCommandStatusUpdate(sagaFlowCommandStatus);
    }

    public async Task HandleSagaFlowCommandSucceeded(SagaFlowCommandStatus sagaFlowCommandStatus)
    {
        await _hub.Clients.All.SendCommandSucceeded(sagaFlowCommandStatus);
    }

    public async Task HandleSagaFlowCommandErrored(SagaFlowCommandStatus sagaFlowCommandStatus)
    {
        await _hub.Clients.All.SendCommandErrored(sagaFlowCommandStatus);
    }

    public async Task ProgressChanged(SagaFlowCommandId commandId, double progress)
    {
        await _hub.Clients.All.SendCommandProgress(new CommandProgressMessage(commandId, progress));
    }
}