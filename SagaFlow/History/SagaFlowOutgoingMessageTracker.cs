using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.ServiceProvider;

namespace SagaFlow.History;

[StepDocumentation("Tracks outgoing messages to record message and saga correlation ids.")]
public class SagaFlowOutgoingMessageTracker : IOutgoingStep
{
    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        // Can be either within a rebus message handling scope (sending another message) or within a SagaFlow command controller posting a new command.
        // TODO: work out how to do this if not using Rebus.ServiceProvider
        var rebusMessageScope = 
            context.GetAsyncServiceScopeOrNull()?.ServiceProvider ??
            context.GetServiceScopeOrNull()?.ServiceProvider ??
            context.Load<IServiceProvider>() ??
            throw new ArgumentException("Could not resolve the IServiceProvider. Are you using Rebus.ServiceProvider?");
        var activityReporter = rebusMessageScope.GetRequiredService<ISagaFlowActivityReporter>();
        var message = context.Load<Message>();
        var messageId = message.Headers.GetValueOrDefault(Rebus.Messages.Headers.MessageId);

        if (messageId == null)
        {
            // TODO: only log a warning?
            throw new ArgumentException("Message does not contain a message id header.");
        }

        // Not every message will be a SagaFlow command.
        var commandIdString = message.Headers.GetValueOrDefault(Headers.SagaFlowCommandId);
        SagaFlowCommandId? commandId = commandIdString != null ? new SagaFlowCommandId(Guid.Parse(commandIdString)) : null;
        var correlationId = message.Headers.GetValueOrDefault(Rebus.Messages.Headers.CorrelationId);

        await activityReporter.RecordMessageSent(messageId, commandId, correlationId, message.Body);

        await next();
    }
}