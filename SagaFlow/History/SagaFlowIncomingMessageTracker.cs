using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Sagas;

namespace SagaFlow.History;

[StepDocumentation("Tracks incoming messages to record message and saga correlation ids.")]
public class SagaFlowIncomingMessageTracker : IIncomingStep
{
    private readonly ISagaFlowActivityStore _activityStore;

    public SagaFlowIncomingMessageTracker(ISagaFlowActivityStore activityStore)
    {
        _activityStore = activityStore;
    }
        
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>();
        var messageId = message.Headers.GetValueOrDefault(Headers.MessageId);

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
        var correlationId = message.Headers.GetValueOrDefault(Headers.CorrelationId);

        if (messageInvokers != null)
        {
            foreach (var messageInvoker in messageInvokers)
            {
                await _activityStore.RecordHandlerStarted(messageId, correlationId, message.Body, messageInvoker.Handler);
            }
        }

        if (sagaInvokers != null)
        {
            foreach (var sagaInvoker in sagaInvokers)
            {
                var sagaId = sagaInvoker.GetSagaData().Id;
                await _activityStore.RecordSagaStepStarted(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga);
            }
        }

        try
        {
            await next();
            
            if (messageInvokers != null)
            {
                foreach (var messageInvoker in messageInvokers)
                {
                    await _activityStore.RecordHandlerFinished(messageId, correlationId, message.Body, messageInvoker.Handler, null);
                }
            }

            if (sagaInvokers != null)
            {
                foreach (var sagaInvoker in sagaInvokers)
                {
                    var sagaId = sagaInvoker.GetSagaData().Id;
                    await _activityStore.RecordSagaStepFinished(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga, null, WasSagaMarkedComplete(sagaInvoker.Saga) );
                }
            }
        }
        catch (Exception ex)
        {
            if (messageInvokers != null)
            {
                foreach (var messageInvoker in messageInvokers)
                {
                    await _activityStore.RecordHandlerFinished(messageId, correlationId, message.Body, messageInvoker.Handler, ex);
                }
            }

            if (sagaInvokers != null)
            {
                foreach (var sagaInvoker in sagaInvokers)
                {
                    var sagaId = sagaInvoker.GetSagaData().Id;
                    await _activityStore.RecordSagaStepFinished(messageId, correlationId, message.Body, sagaId, sagaInvoker.Saga, ex, WasSagaMarkedComplete(sagaInvoker.Saga));
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