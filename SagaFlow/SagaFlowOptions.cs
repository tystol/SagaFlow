using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing;
using Rebus.Sagas;
using Rebus.ServiceProvider;
using Rebus.Subscriptions;
using Rebus.Timeouts;
using Rebus.Transport;
using SagaFlow.Schema;

namespace SagaFlow
{
    public class SagaFlowOptions
    {
        internal Action<RebusLoggingConfigurer> LoggingConfigurer { get; private set; }
        internal Action<StandardConfigurer<ITransport>> TransportConfigurer { get; private set; }
        internal Action<StandardConfigurer<IRouter>> RoutingConfigurer { get; private set; }
        internal Action<StandardConfigurer<ISagaStorage>> SagaConfigurer { get; private set; }
        internal Action<StandardConfigurer<ISubscriptionStorage>> SubscriptionConfigurer { get; private set; }
        internal Action<StandardConfigurer<ITimeoutManager>> TimeoutConfigurer { get; private set; }
        internal List<Action<IServiceCollection>> HandlerSetup { get; } = new List<Action<IServiceCollection>>();
        internal List<Func<IEnumerable<Type>>> ResourceProviderTypes { get; } = new List<Func<IEnumerable<Type>>>();
        internal List<Func<IEnumerable<Command>>> Commands { get; } = new List<Func<IEnumerable<Command>>>();
        internal List<Func<IEnumerable<Type>>> CommandTypes { get; } = new List<Func<IEnumerable<Type>>>();

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
                .Where(t => !t.IsAbstract && t.IsImplementationOfOpenGenericInterface(typeof(IResourceListProvider<>))));
            return this;
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
    }
}
