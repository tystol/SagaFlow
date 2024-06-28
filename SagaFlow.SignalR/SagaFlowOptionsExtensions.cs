using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SagaFlow.AspNetCore.Formatters;
using SagaFlow.SignalR.Hubs;
using SagaFlow.Status;

namespace SagaFlow.SignalR;

public static class SagaFlowOptionsExtensions
{
    public static SagaFlowOptions WithSignalR(this SagaFlowOptions options)
    {
        options
            .AddService(services =>
                services
                    .AddTransient<ISagaFlowCommandStateChangedHandler, PublishSagaFlowCommandStateChangeToSignalRHub>()
                    .AddTransient<ISagaFlowCommandErroredHandler, PublishSagaFlowCommandStateChangeToSignalRHub>()
                    .AddTransient<ISagaFlowCommandSucceededHandler, PublishSagaFlowCommandStateChangeToSignalRHub>())
            
            .AddService(services =>
                services.AddSignalR()
                    .AddJsonProtocol(jsonHubProtocolOptions => jsonHubProtocolOptions.PayloadSerializerOptions = SagaFlowCommandStatusJsonSerializerOptions.JsonSerializerOptions));

        options.AddHostSetup(
            host =>
            {
                if (host is not IEndpointRouteBuilder endpointBuilder)
                {
                    throw new InvalidOperationException(
                        "SagaFlow SignalR can only be added to an AspNet.Core application");
                }
                    
                var sagaFlowModule = endpointBuilder.ServiceProvider.GetRequiredService<SagaFlowModule>();

                endpointBuilder.MapHub<SagaFlowSignalRHub>($"/{sagaFlowModule.ApiBasePath}/update-hub");
            });
        
        return options;
    }
}