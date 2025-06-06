﻿using Microsoft.AspNetCore.SignalR;
using SagaFlow.History;

namespace SagaFlow.SignalR.Hubs;

public interface ISagaFlowSignalRHub
{
    Task SendCommandProgress(CommandProgressMessage commandProgress);
    Task SendCommandStatusUpdate(SagaFlowCommandStatus commandStatus);
    
    Task SendCommandSucceeded(SagaFlowCommandStatus commandStatus);
    
    Task SendCommandErrored(SagaFlowCommandStatus commandStatus);
}

public class SagaFlowSignalRHub : Hub<ISagaFlowSignalRHub>
{
    public async Task SendCommandProgress(CommandProgressMessage message)
    {
        await Clients.All.SendCommandProgress(message);
    }
    
    public async Task SendCommandStatusUpdate(SagaFlowCommandStatus commandStatus)
    {
        await Clients.All.SendCommandStatusUpdate(commandStatus);
    }
    
    public async Task SendCommandSucceeded(SagaFlowCommandStatus commandStatus)
    {
        await Clients.All.SendCommandSucceeded(commandStatus);
    }
    
    public async Task SendCommandErrored(SagaFlowCommandStatus commandStatus)
    {
        await Clients.All.SendCommandErrored(commandStatus);
    }
}

public record CommandProgressMessage(SagaFlowCommandId CommandId, double Progress);
