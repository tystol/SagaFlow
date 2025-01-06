using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus.Messages;
using Rebus.Pipeline;

namespace SagaFlow.History;

[StepDocumentation("Tracks outgoing messages to record message and saga correlation ids.")]
public class SagaFlowOutgoingMessageTracker : IOutgoingStep
{
    private readonly ISagaFlowActivityStore _activityStore;

    public SagaFlowOutgoingMessageTracker(ISagaFlowActivityStore activityStore)
    {
        _activityStore = activityStore;
    }

    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>();
        var messageId = message.Headers.GetValueOrDefault(Headers.MessageId);

        if (messageId == null)
        {
            // TODO: only log a warning?
            throw new ArgumentException("Message does not contain a message id header.");
        }

        // Not every message will be a SagaFlow command.
        var commandIdString = message.Headers.GetValueOrDefault(SagaFlowRebusEvents.SagaFlowCommandId);
        SagaFlowCommandId? commandId = commandIdString != null ? new SagaFlowCommandId(Guid.Parse(commandIdString)) : null;
        var correlationId = message.Headers.GetValueOrDefault(Headers.CorrelationId);

        await _activityStore.RecordMessageSent(messageId, commandId, correlationId, message.Body);

        await next();
    }
}