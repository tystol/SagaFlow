using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rebus.Pipeline;
using SagaFlow;
using SagaFlow.AspNetCore.Authentications;
using SagaFlow.AspNetCore.Rebus;
using SagaFlow.Authentications;
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

        services.AddHttpContextAccessor();
        
        var sagaFlowModule = SagaFlowModuleFactory.Create(options, apiBasePath);

        services.AddSagaFlowCore(options, sagaFlowModule, i =>
        {
            var httpAccessor = sagaFlowModule.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            return i.OnSend(new PerHttpRequestScopeInjector(httpAccessor), PipelineRelativePosition.Before,
                typeof(Rebus.Pipeline.Send.AssignDefaultHeadersStep));
        });
        
        services.Configure<MvcOptions>(o => o.Conventions.Add(new MvcEndpointRouteConvention(sagaFlowModule)));
        var manager = GetServiceFromCollection<ApplicationPartManager>(services);
        manager.FeatureProviders.Add(new MvcEndpointProvider(sagaFlowModule));

        return services;
    }

    
    /// <summary>
    /// The username to use when there is not names claim on the User principal submitting the job.  Default value is 'Anonymous'.
    /// The value is ignored if a custom implementation of IUsernameProvider is used.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public static SagaFlowOptions WithAnonymousUsername(this SagaFlowOptions options, string username) => 
        options.AddSetupContext(nameof(HttpContextUsernameProvider.AnonymousUserName), username);
    
    /// <summary>
    /// The username to use when the system submits a job, for example when CRON jobs are scheduled and executed.  The Default value is 'System'.
    /// The value is ignored if a custom implementation of IUsernameProvider is used.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public static SagaFlowOptions WithSystemUsername(this SagaFlowOptions options, string username) => 
        options.AddSetupContext(nameof(HttpContextUsernameProvider.SystemUsername), username);
    
    /// <summary>
    /// Provide a custom implementation of IUsernameProvider so SagaFlow can associate a username to Commands that have
    /// been triggered to run.
    /// </summary>
    /// <param name="options"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static SagaFlowOptions WithUsernameProvider<T>(this SagaFlowOptions options) where T: class, IUsernameProvider  => 
        options.AddService(s => s.AddScoped<IUsernameProvider, T>());
    
    private static T GetServiceFromCollection<T>(IServiceCollection services)
    {
        return (T)services
            .LastOrDefault(d => d.ServiceType == typeof(T))
            ?.ImplementationInstance;
    }
}