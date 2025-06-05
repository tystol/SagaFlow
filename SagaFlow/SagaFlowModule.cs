using System;
using System.Collections.Generic;
using SagaFlow.Schema;

namespace SagaFlow
{
    public class SagaFlowModule : ISagaFlowSchemaProvider
    {
        public string ApiBasePath { get; init; }
        public IReadOnlyList<Command> Commands { get; init; }
        public IReadOnlyList<ResourceProvider> ResourceProviders { get; init; }

        private IServiceProvider? serviceProvider;
        internal IServiceProvider ServiceProvider
        {
            get
            {
                if (serviceProvider == null)
                    throw new InvalidOperationException("SagaFlow service provider is not configured. Are you missing a call to 'app.UseSagaFlow()'?");
                return serviceProvider;
            }
            set => serviceProvider = value;
        }

        internal List<Action<object>> SageFlowStartup = [];
    }
}
