using System.Collections.Generic;
using SagaFlow.Schema;

namespace SagaFlow
{
    public interface ISagaFlowSchemaProvider
    {
        IReadOnlyList<ResourceProvider> ResourceProviders { get; }
        IReadOnlyList<Command> Commands { get; }
    }
}
