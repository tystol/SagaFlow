﻿using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SagaFlow.AspNetCore.Formatters;
using SagaFlow.SignalR.Hubs;
using SagaFlow.History;

namespace SagaFlow.SignalR;

public static class SagaFlowOptionsExtensions
{
    public static SagaFlowOptions WithSignalR(this SagaFlowOptions options)
    {
        // TODO: better way to extend services within SagaFlow extensions.
        options.AddService(services =>
            services
                .AddTransient<ISagaFlowEventHandler, PublishSagaFlowEventsToSignalRHub>()
                .AddSignalR()
                    .AddJsonProtocol(jsonHubProtocolOptions => jsonHubProtocolOptions.PayloadSerializerOptions =
                    SagaFlowCommandStatusJsonSerializerOptions.JsonSerializerOptions));
            

        options.AddHostSetup(
            host =>
            {
                var endpointBuilder = GetEndpointBuilder(host);
                    
                var sagaFlowModule = endpointBuilder.ServiceProvider.GetRequiredService<SagaFlowModule>();

                endpointBuilder.MapHub<SagaFlowSignalRHub>($"/{sagaFlowModule.ApiBasePath}/update-hub");
            });
        
        return options;
    }

    private static IEndpointRouteBuilder GetEndpointBuilder(object hostBuilder)
    {
        if (hostBuilder is IEndpointRouteBuilder endpointBuilder)
        {
            return endpointBuilder;
        }
        
        if (hostBuilder is IApplicationBuilder applicationBuilder)
        {
            endpointBuilder = applicationBuilder.Properties
                .Select(p => p.Value)
                .FirstOrDefault(value => value is IEndpointRouteBuilder) as IEndpointRouteBuilder ?? throw new InvalidOperationException(
                "SagaFlow SignalR requires routing");

            return endpointBuilder;
        }
        
        throw new InvalidOperationException(
            "SagaFlow SignalR can only be added to an AspNet.Core application");
    }
}