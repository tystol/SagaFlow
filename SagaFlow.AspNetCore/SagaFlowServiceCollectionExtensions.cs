using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SagaFlow;
using SagaFlow.AspNetCore.Authentications;
using SagaFlow.Authentications;
using SagaFlow.MvcProvider;
using SagaFlow.Utilities;


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

        // If no custom IUsername Provider was added via the SagaFlowOptions then
        // register the a UsernameProvider using the current Identity of the user
        // of the HttpContext's request.
        services.AddHttpContextAccessor();
        services.TryAddScoped<IUsernameProvider>(s =>
        {
            var httpContextUsernameProvider = ActivatorUtilities.CreateInstance<HttpContextUsernameProvider>(s);
            
            if (options.SetupContext.TryGetContextValue<string>(nameof(HttpContextUsernameProvider.SystemUsername), out var systemUserName))
            {
                httpContextUsernameProvider.SystemUsername = systemUserName;
            }
            
            if (options.SetupContext.TryGetContextValue<string>(nameof(HttpContextUsernameProvider.AnonymousUserName), out var anonymousUserName))
            {
                httpContextUsernameProvider.AnonymousUserName = anonymousUserName;
            }

            return httpContextUsernameProvider;
        });
        
        var sagaFlowModule = SagaFlowModuleFactory.Create(options, apiBasePath);

        services.AddSagaFlowCore(options, sagaFlowModule);
        
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