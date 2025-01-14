using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Sagas;

namespace SagaFlow.History;

[StepDocumentation("Tracks incoming messages to record message and saga correlation ids.")]
public class SagaFlowIncomingMessageTracker : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        // TODO: work out how to do this if not using Rebus.ServiceProvider
        var rebusMessageScope = context.Load<AsyncServiceScope?>() ?? context.Load<IServiceScope>() ??
            throw new ArgumentException("Could not resolve the IServiceScope. Are you using Rebus.ServiceProvider?");
        var activityReporter = rebusMessageScope.ServiceProvider.GetRequiredService<ISagaFlowActivityReporter>();
        
        var message = context.Load<Message>();
        var messageId = message.Headers.GetValueOrDefault(Rebus.Messages.Headers.MessageId);

        if (messageId == null)
        {
            // TODO: only log a warning?
            throw new ArgumentException("Message does not contain a message id header.");
        }
        
        var allHandlers = context.Load<HandlerInvokers>()
            .GroupBy(h => h.HasSaga)
            .ToDictionary(g => g.Key, g => g.ToList());
        var messageInvokers = allHandlers.GetValueOrDefault(false);
        var sagaInvokers = allHandlers.GetValueOrDefault(true);
        var correlationId = message.Headers.GetValueOrDefault(Rebus.Messages.Headers.CorrelationId);

        var transportMessage = context.Load<TransportMessage>() ?? throw new ArgumentException("Could not find a transport message in the current incoming step context");
        if (transportMessage.Headers.TryGetValue(Rebus.Messages.Headers.DeliveryCount, out var value) && int.TryParse(value, out var deliveryCount))
        {
            // TODO: this doesnt seem to be reliably populated (I think dependant on transport layer) so maybe add our
            // own command attempt counter?
        }
        
        if (messageInvokers != null)
        {
            foreach (var messageInvoker in messageInvokers)
            {
                await activityReporter.RecordHandlerStarted(messageId, correlationId, message.Body, messageInvoker.Handler);
            }
        }

        if (sagaInvokers != null)
        {
            foreach (var sagaInvoker in sagaInvokers)
            {
                var sagaId = sagaInvoker.GetSagaData().Id;
                await activityReporter.RecordSagaStepStarted(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga);
            }
        }

        try
        {
            await next();
            
            if (messageInvokers != null)
            {
                foreach (var messageInvoker in messageInvokers)
                {
                    await activityReporter.RecordHandlerFinished(messageId, correlationId, message.Body, messageInvoker.Handler, null);
                }
            }

            if (sagaInvokers != null)
            {
                foreach (var sagaInvoker in sagaInvokers)
                {
                    var sagaId = sagaInvoker.GetSagaData().Id;
                    await activityReporter.RecordSagaStepFinished(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga, null, WasSagaMarkedComplete(sagaInvoker.Saga) );
                }
            }
        }
        catch (Exception ex)
        {
            if (messageInvokers != null)
            {
                foreach (var messageInvoker in messageInvokers)
                {
                    await activityReporter.RecordHandlerFinished(messageId, correlationId, message.Body, messageInvoker.Handler, ex);
                }
            }

            if (sagaInvokers != null)
            {
                foreach (var sagaInvoker in sagaInvokers)
                {
                    var sagaId = sagaInvoker.GetSagaData().Id;
                    await activityReporter.RecordSagaStepFinished(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga, ex, WasSagaMarkedComplete(sagaInvoker.Saga));
                }
            }
            
            throw;
        }
    }

    private static readonly PropertyInfo? WasMarkedAsCompleteProperty = typeof(Saga).GetProperty("WasMarkedAsComplete",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    internal static bool WasSagaMarkedComplete(Saga saga)
    {
        if (WasMarkedAsCompleteProperty == null)
            throw new ArgumentException("Could not find Saga.WasMarkedAsComplete property. Rebus internals may have changed.");
        
        return (bool) (WasMarkedAsCompleteProperty.GetValue(saga) ?? throw new InvalidOperationException("Saga.WasMarkedAsComplete returned null. Rebus internals may have changed."));
    }
}