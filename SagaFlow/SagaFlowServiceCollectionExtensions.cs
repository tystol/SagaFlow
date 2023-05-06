using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using SagaFlow;
using SagaFlow.MvcProvider;
using SagaFlow.Schema;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SagaFlowServiceCollectionExtensions
    {
        public static IServiceCollection AddSagaFlow(
            this IServiceCollection services,
            Action<SagaFlowOptions> setupAction,
            string apiBasePath = "sagaflow")
        {
            var options = new SagaFlowOptions();
            setupAction(options);

            // Potentiially use this method if properly registering via Options
            //services.Configure(setupAction);

            // Register saga/workflow handlers
            foreach (var handlerConfig in options.HandlerSetup)
            {
                handlerConfig(services);
            }

            // when added here, the bus will be NOT have been disposed when the StopAsync method gets called
            //services.AddHostedService<BackgroundServiceExample>();

            //var commandTypes = options.CommandTypes.SelectMany(f => f()).ToList();
            var commands = options.Commands.SelectMany(f => f())
                .Concat(options.CommandTypes.SelectMany(ct => ct().Select(BuildCommandFromType)))
                .ToList();
            var resourceDefinitions = options.ResourceProviderTypes.SelectMany(f => f())
                .SelectMany(BuildResourcesFromProviderType)
                .ToList();
            var sagaFlowModule = new SagaFlowModule
            {
                ApiBasePath = apiBasePath,
                Commands = commands,
                ResourceProviders = resourceDefinitions,
            };
            services.TryAddSingleton(sagaFlowModule);
            services.TryAddSingleton<ISagaFlowSchemaProvider>(sagaFlowModule);

            // Configure and register Rebus
            services.AddRebus(c =>
            {
                // TODO split saga client vs host, similar to this:
                // https://github.com/rebus-org/RebusSamples/blob/1e18159f39bdce3f7a36dff022750908f602d7b5/Sagas/Common/CommonRebusConfigurationExtensions.cs#L10

                if (options.LoggingConfigurer != null)
                    c = c.Logging(options.LoggingConfigurer);

                c = c.Transport(options.TransportConfigurer);

                if (options.RoutingConfigurer == null)
                {
                    c = c.Routing(r => r.TypeBased().MapFallback("sagaflow.messages"));
                }
                else
                    c = c.Routing(options.RoutingConfigurer);
        
                if (options.SubscriptionConfigurer != null)
                    c = c.Subscriptions(options.SubscriptionConfigurer);
                if (options.SagaConfigurer != null)
                    c = c.Sagas(options.SagaConfigurer);
                if (options.TimeoutConfigurer != null)
                    c = c.Timeouts(options.TimeoutConfigurer);

                return c;
            });


            services.Configure<MvcOptions>(o => o.Conventions.Add(new MvcEndpointRouteConvention(sagaFlowModule)));
            var manager = GetServiceFromCollection<ApplicationPartManager>(services);
            manager.FeatureProviders.Add(new MvcEndpointProvider(sagaFlowModule));

            // Register custom configurators that takes values from SwaggerGenOptions (i.e. high level config)
            // and applies them to SwaggerGeneratorOptions and SchemaGeneratorOptoins (i.e. lower-level config)
            //services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            //services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

            // Register generator and it's dependencies
            /*
            services.TryAddSingleton<IDocumentProvider, DocumentProvider>();
            services.TryAddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.TryAddTransient(s => s.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value);
            services.TryAddTransient<ISchemaGenerator, SchemaGenerator>();
            services.TryAddTransient(s => s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value);
            services.TryAddTransient<ISerializerDataContractResolver>(s =>
            {
#if (!NETSTANDARD2_0)
                var serializerOptions = s.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
                    ?? new JsonSerializerOptions();
#else
                var serializerOptions = new JsonSerializerOptions();
#endif

                return new JsonSerializerDataContractResolver(serializerOptions);
            });

            services.Configure(setupAction);
            */

            return services;
        }


        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }

        private static IEnumerable<ResourceProvider> BuildResourcesFromProviderType(Type resourceProviderType)
        {
            foreach (var resourceProviderInterface in resourceProviderType.GetInterfacesOfOpenGenericInterface(typeof(IResourceListProvider<>)))
            {
                var resourceType = resourceProviderInterface.GetGenericArguments()[0];
                var resourceTypeAttributes = resourceType.GetCustomAttributes();
                var displayNameAttribute = resourceTypeAttributes.OfType<DisplayNameAttribute>().FirstOrDefault();

                // TODO: Should probably use something else - DisplayName on a singular resource type DTO doesn't
                // quite fit, as the resource type represents a single, where as the below resource provider is more
                // a plural version.
                var resourceName = displayNameAttribute?.DisplayName ?? resourceType.Name + "s";
                yield return new ResourceProvider
                {
                    Id = resourceName.ToKebabCase(),
                    Type = resourceType,
                    Name = resourceName,
                    ProviderType = resourceProviderType,
                    IdType = null, // todo
                };
            }
        }

        private static Command BuildCommandFromType(Type commandType)
        {
            var parameterProps = commandType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => new
                {
                    PropertyInfo = p,
                    Attributes = p.GetCustomAttributes()
                })
                //.Where(p => p.Attributes.Any(a => a is IgnoreAttribute)
                .ToList();

            var commandTypeAttributes = commandType.GetCustomAttributes();
            var displayNameAttribute = commandTypeAttributes.OfType<DisplayNameAttribute>().FirstOrDefault();

            return new Command
            {
                Id = commandType.Name.ToKebabCase(),
                CommandType = commandType,
                Name = displayNameAttribute?.DisplayName ?? commandType.Name,
                EventType = null,
                Parameters = parameterProps.Select(p => new CommandParameter
                {
                    Id = p.PropertyInfo.Name, // TODO: default to camelCase for property ids to match json / front end js conventions?
                    Name = p.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? p.PropertyInfo.Name,
                    Description = p.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description,
                    InputType = p.PropertyInfo.PropertyType,
                })
                .ToList(),
            };
        }
    }
}
