using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using SagaFlow;
using SagaFlow.MvcProvider;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SagaFlowServiceCollectionExtensions
{
    public static IServiceCollection AddSagaFlow( 
        this IServiceCollection services,
        Action<SagaFlowOptions> setupAction,
        string apiBasePath = "sagaflow")
    {
        var options = new SagaFlowOptions();
        setupAction(options);
        
        var sagaFlowModule = SagaFlowModuleFactory.Create(options, apiBasePath);

        services.AddSagaFlowCore(options, sagaFlowModule);
        
        services.Configure<MvcOptions>(o => o.Conventions.Add(new MvcEndpointRouteConvention(sagaFlowModule)));
        var manager = GetServiceFromCollection<ApplicationPartManager>(services);
        manager.FeatureProviders.Add(new MvcEndpointProvider(sagaFlowModule));

        return services;
    }
    
    private static T GetServiceFromCollection<T>(IServiceCollection services)
    {
        return (T)services
            .LastOrDefault(d => d.ServiceType == typeof(T))
            ?.ImplementationInstance;
    }
}