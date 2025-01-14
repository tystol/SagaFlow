using System;
using System.Threading.Tasks;

namespace SagaFlow.History;

public interface ISagaFlowActivityReporter
{
    /// <summary>
    /// Records a command message being sent via the SagaFlow API. 
    /// </summary>
    /// <remarks>
    /// This is separate from every message sent over the message bus. eg. a command may initiate a handler or saga,
    /// that then triggers multiple other messages to be sent. Only this first command message sent via the SagaFlow
    /// API is treated as an actual command. All other messages are just correlated messages that are linked back
    /// to this initiating command.
    /// </remarks>
    Task RecordCommandInitiated(SagaFlowCommandId commandId, object commandMessage);

    Task RecordMessageSent(SagaFlowMessageId messageId, SagaFlowCommandId? commandId, string? correlationId, object message);

    Task RecordHandlerStarted(SagaFlowMessageId messageId, string? correlationId, object message, object handler);

    Task RecordHandlerFinished(SagaFlowMessageId messageId, string? correlationId, object message, object handler, Exception? error);

    Task RecordSagaStepStarted(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga);

    Task RecordSagaStepFinished(SagaFlowMessageId messageId, string? correlationId, object message, SagaFlowSagaId sagaId, object saga, Exception? error, bool sagaFinished);

    Task RecordCommandProgress(SagaFlowCommandId commandId, double progress);
}
