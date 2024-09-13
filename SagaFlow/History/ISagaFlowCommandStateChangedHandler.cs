using System.Threading.Tasks;

namespace SagaFlow.History;

public interface ISagaFlowCommandStateChangedHandler
{
    Task HandleSagaFlowCommandStatusUpdate(SagaFlowCommandStatus sagaFlowCommandStatus);
}

public interface ISagaFlowCommandSucceededHandler
{
    Task HandleSagaFlowCommandSucceeded(SagaFlowCommandStatus sagaFlowCommandStatus);
}

public interface ISagaFlowCommandErroredHandler
{
    Task HandleSagaFlowCommandErrored(SagaFlowCommandStatus sagaFlowCommandStatus);
}