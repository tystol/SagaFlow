using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Config;
using Rebus.Routing;
using Rebus.Sagas;
using Rebus.Subscriptions;
using Rebus.Timeouts;
using Rebus.Transport;
using SagaFlow.Schema;
using SagaFlow.Status;

namespace SagaFlow
{
    public class SagaFlowOptions
    {
        internal Action<OptionsConfigurer> OptionsConfigurer { get; private set; }
        internal Action<RebusLoggingConfigurer> LoggingConfigurer { get; private set; }
        internal Action<StandardConfigurer<ITransport>> TransportConfigurer { get; private set; }
        internal Action<StandardConfigurer<IRouter>> RoutingConfigurer { get; private set; }
        internal Action<StandardConfigurer<ISagaStorage>> SagaConfigurer { get; private set; }
        internal Action<StandardConfigurer<ISubscriptionStorage>> SubscriptionConfigurer { get; private set; }
        internal Action<StandardConfigurer<ITimeoutManager>> TimeoutConfigurer { get; private set; }
        internal List<Action<IServiceCollection>> HandlerSetup { get; } = new List<Action<IServiceCollection>>();
        
        internal List<Action<object>> HostHandlerSetup { get; } = new List<Action<object>>();
        internal List<Func<IEnumerable<Type>>> ResourceProviderTypes { get; } = new List<Func<IEnumerable<Type>>>();
        internal List<Func<IEnumerable<Command>>> Commands { get; } = new List<Func<IEnumerable<Command>>>();
        internal List<Func<IEnumerable<Type>>> CommandTypes { get; } = new List<Func<IEnumerable<Type>>>();

        internal IDictionary<string, object> SetupContext = new Dictionary<string, object>();

        public SagaFlowOptions WithOptions(Action<OptionsConfigurer> configurer)
        {
            OptionsConfigurer = configurer;

            return this;
        }

        public SagaFlowOptions WithLogging(Action<RebusLoggingConfigurer> configurer)
        {
            LoggingConfigurer = configurer;
            return this;
        }

        public SagaFlowOptions WithTransport(Action<StandardConfigurer<ITransport>> configurer)
        {
            TransportConfigurer = configurer;
            return this;
        }

        public SagaFlowOptions WithRouting(Action<StandardConfigurer<IRouter>> configurer)
        {
            RoutingConfigurer = configurer;
            return this;
        }

        public SagaFlowOptions WithSagaStorage(Action<StandardConfigurer<ISagaStorage>> configurer)
        {
            SagaConfigurer = configurer;
            return this;
        }

        public SagaFlowOptions WithSubscriptionStorage(Action<StandardConfigurer<ISubscriptionStorage>> configurer)
        {
            SubscriptionConfigurer = configurer;
            return this;
        }

        public SagaFlowOptions WithTimeoutStorage(Action<StandardConfigurer<ITimeoutManager>> configurer)
        {
            TimeoutConfigurer = configurer;
            return this;
        }

        /// <summary>
        /// Adds a custom implementation of TCommandStatusStore to replace the default In-memory implementation.
        /// </summary>
        /// <typeparam name="TCommandStatusStore"></typeparam>
        /// <returns></returns>
        public SagaFlowOptions WithCustomCommandStatusStore<TCommandStatusStore>() where TCommandStatusStore : class, ISagaFlowCommandStore
        {
            HandlerSetup.Add((s) => s.AddTransient<ISagaFlowCommandStore, TCommandStatusStore>());

            return this;
        }
        
        /// <summary>
        /// Adds a custom handler when the command status has been updated
        /// </summary>
        /// <typeparam name="TCommandStatusChangedHandler"></typeparam>
        /// <returns></returns>
        public SagaFlowOptions AddCustomCommandStatusChangedHandler<TCommandStatusChangedHandler>() where TCommandStatusChangedHandler : class, ISagaFlowCommandStateChangedHandler
        {
            HandlerSetup.Add((s) => s.AddTransient<ISagaFlowCommandStateChangedHandler, TCommandStatusChangedHandler>());

            return this;
        }
        
        /// <summary>
        /// Adds a custom handler when a command has succeeded.
        /// </summary>
        /// <typeparam name="TSagaFlowCommandSucceededHandler"></typeparam>
        /// <returns></returns>
        public SagaFlowOptions AddCustomCommandSucceededHandler<TSagaFlowCommandSucceededHandler>() where TSagaFlowCommandSucceededHandler : class, ISagaFlowCommandSucceededHandler
        {
            HandlerSetup.Add((s) => s.AddTransient<ISagaFlowCommandSucceededHandler, TSagaFlowCommandSucceededHandler>());

            return this;
        }
        
        /// <summary>
        /// Adds a customer handler when a command has errored
        /// </summary>
        /// <typeparam name="TSagaFlowCommandErroredHandler"></typeparam>
        /// <returns></returns>
        public SagaFlowOptions AddCustomCommandErroredHandler<TSagaFlowCommandErroredHandler>() where TSagaFlowCommandErroredHandler : class, ISagaFlowCommandErroredHandler
        {
            HandlerSetup.Add((s) => s.AddTransient<ISagaFlowCommandErroredHandler, TSagaFlowCommandErroredHandler>());

            return this;
        }

        public SagaFlowOptions AddHandlersFromAssemblyOf<T>()
        {
            HandlerSetup.Add(services => services.AutoRegisterHandlersFromAssemblyOf<T>());
            return this;
        }

        public SagaFlowOptions AddWorkflowsFromAssemblyOf<T>()
        {
            return AddHandlersFromAssemblyOf<T>();
        }

        public SagaFlowOptions AddResourceProvidersFromAssemblyOf<T>()
        {
            ResourceProviderTypes.Add(() => typeof(T).Assembly.GetTypes()
                .Where(IsResourceProviderType));
            return this;

            bool IsResourceProviderType(Type type)
            {
                return !type.IsAbstract && (
                    type.IsImplementationOfOpenGenericInterface(typeof(IResourceListProvider<>)) ||
                    type.IsImplementationOfOpenGenericInterface(typeof(IResourceListProvider<,>))
                );
            }
        }
        
        public SagaFlowOptions AddCommandFromEvent<T>()
        {
            var eventType = typeof(T);
            var eventName = eventType.Name;
            // TODO: support attribute mapping
            var commandName = eventName.EndsWith("Requested", StringComparison.InvariantCultureIgnoreCase) ?
                eventName.Substring(0, eventName.Length - 9) :
                eventName;

            Commands.Add(() => new[] { new Command
            {
                Name = commandName,
                EventType = eventType,
                CommandType = typeof(Newtonsoft.Json.Linq.JObject),
            } });
            return this;
        }
        
        public SagaFlowOptions AddCommandsOfType<T>()
        {
            CommandTypes.Add(() => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(T))));
            return this;
        }

        internal SagaFlowOptions AddService(Action<IServiceCollection> handler)
        {
            this.HandlerSetup.Add(handler);

            return this;
        }

        internal SagaFlowOptions AddHostSetup(Action<object> hostSetupHandler)
        {
            this.HostHandlerSetup.Add(hostSetupHandler);

            return this;
        }

        internal SagaFlowOptions AddSetupContext<T>(string key, T value)
        {
            this.SetupContext.TryAdd(key, value);

            return this;
        }
    }
}
