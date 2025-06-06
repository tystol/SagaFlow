using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using SagaFlow.Authentications;
using SagaFlow.Recurring;
using SagaFlow.Schema;
using SagaFlow.History;
using SagaFlow.Time;

namespace SagaFlow;

public static class SagaFlowServiceCollectionExtensions
{
    public static IServiceCollection AddSagaFlowCore(
        this IServiceCollection services,
        SagaFlowOptions options,
        SagaFlowModule sagaFlowModule,
        Func<PipelineStepInjector,PipelineStepInjector> pipelineAdditions)
    {
        // Register saga/workflow handlers
        foreach (var handlerConfig in options.HandlerSetup) handlerConfig(services);
        
        RegisterResourceProviderInServiceCollection(services, sagaFlowModule.ResourceProviders.ToList());

        services.TryAddSingleton(sagaFlowModule);
        services.TryAddSingleton<ISagaFlowSchemaProvider>(sagaFlowModule);
        services.AddTransient<IHumanReadableCommandPropertiesResolver, HumanReadableCommandPropertiesResolver>();
        services.AddTransient<IHumanReadableCommandNameResolver, HumanReadableCommandNameResolver>();
        services.AddTransient<ISagaFlowCommandBus, RebusCommandBus>();

        // Configure and register Rebus
        services.AddRebus(c =>
        {
            // TODO split saga client vs host, similar to this:
            // https://github.com/rebus-org/RebusSamples/blob/1e18159f39bdce3f7a36dff022750908f602d7b5/Sagas/Common/CommonRebusConfigurationExtensions.cs#L10

            if (options.OptionsConfigurer != null)
                c = c.Options(options.OptionsConfigurer);

            if (options.LoggingConfigurer != null)
                c = c.Logging(options.LoggingConfigurer);

            c = c.Transport(options.TransportConfigurer);

            if (options.RoutingConfigurer == null)
                c = c.Routing(r => r.TypeBased().MapFallback("sagaflow.messages"));
            else
                c = c.Routing(options.RoutingConfigurer);

            if (options.SubscriptionConfigurer != null)
                c = c.Subscriptions(options.SubscriptionConfigurer);
            if (options.SagaConfigurer != null)
                c = c.Sagas(options.SagaConfigurer);
            if (options.TimeoutConfigurer != null)
                c = c.Timeouts(options.TimeoutConfigurer);

            c.ConfigureSagaFlowEventsForRebus(sagaFlowModule, pipelineAdditions);

            return c;
        });
            // TODO: decide on direct send vs pub/sub command pattern
            /*
            ,
            onCreated: bus =>
            {
                return Task.WhenAll(sagaFlowModule.Commands.Select(c => bus.Subscribe(c.CommandType)));
            });
            */

        services.AddTransient<ISagaFlowCommandContext, SagaFlowRebusCommandContext>();
        
        // Fallback registration of the InMemorySagaFlowCommandStore if no ISagaFlowCommandStore was registered
        services.TryAddTransient<ISagaFlowActivityReporter, InMemorySagaFlowActivityStore>();
        services.TryAddTransient<ISagaFlowActivityStore, InMemorySagaFlowActivityStore>();
        
        // Fallback registration of IUsernameProvider
        services.TryAddSingleton<IUsernameProvider, StubUsernameProvider>();
        
        services.TryAddSingleton<ISagaFlowTime, DefaultSagaFlowTime>();

        services.AddSingleton<CronRecurringCommandsBackgroundService>();
        services.AddSingleton<IRecurringCommandScopeAccessor>(c => c.GetRequiredService<CronRecurringCommandsBackgroundService>());
        services.AddHostedService(c => c.GetRequiredService<CronRecurringCommandsBackgroundService>());
        return services;
    }

    private static void RegisterResourceProviderInServiceCollection(IServiceCollection services,
        List<ResourceProvider> resourceDefinitions)
    {
        foreach (var resourceDefinition in resourceDefinitions)
        {
            var resourceListProviderType = typeof(IResourceListProvider<>).MakeGenericType(resourceDefinition.Type);

            // Try to auto-register the resource provider, in-case the consuming application hasn't already registered
            // a custom registration
            services.TryAddScoped(resourceListProviderType, resourceDefinition.ProviderType);
        }
    }
}