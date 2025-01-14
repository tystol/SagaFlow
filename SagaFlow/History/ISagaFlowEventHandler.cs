using System.Threading.Tasks;

namespace SagaFlow.History;

public interface ISagaFlowEventHandler
{
    Task ProgressChanged(SagaFlowCommandId commandId, double progress);
}