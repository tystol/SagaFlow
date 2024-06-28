using Rebus.Config;

namespace SagaFlow.Status;

internal static class SagaFlowStatusConfiguration
{
    internal static RebusConfigurer ConfigureSagaFlowEventsForRebus(this RebusConfigurer configure, SagaFlowModule sagaFlowModule)
    {
        return configure.Events(events =>
        {
            events.BeforeMessageSent += SagaFlowRebusEvents.OnBeforeMessageSent(sagaFlowModule);
            events.BeforeMessageHandled += SagaFlowRebusEvents.OnBeforeMessageHandled(sagaFlowModule);
            events.AfterMessageHandled += SagaFlowRebusEvents.OnAfterMessageHandled(sagaFlowModule);
        });
    }
}