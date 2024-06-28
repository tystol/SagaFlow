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
        internal IServiceProvider ServiceProvider { get; set; }

        internal List<Action<object>> SageFlowStartup = new List<Action<object>>();
    }
}
