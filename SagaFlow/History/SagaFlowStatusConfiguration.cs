using System;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline.Send;
using SagaFlow.Recurring;

namespace SagaFlow.History;

internal static class SagaFlowStatusConfiguration
{
    internal static RebusConfigurer ConfigureSagaFlowEventsForRebus(this RebusConfigurer configure, SagaFlowModule sagaFlowModule, Func<PipelineStepInjector,PipelineStepInjector>? pipelineAdditions)
    {
        return configure
            .Options(opts =>
            {
                //opts.LogPipeline(verbose:true);
                opts.Decorate<IPipeline>(c =>
                {
                    var pipeline = c.Get<IPipeline>();
                    var sendMessageTracker = new SagaFlowOutgoingMessageTracker();
                    var receiveMessageTracker = new SagaFlowIncomingMessageTracker();
                    var scopeAccessor = sagaFlowModule.ServiceProvider.GetRequiredService<IRecurringCommandScopeAccessor>();
                    var recurringCommandScopeInjector = new RecurringCommandScopeInjector(scopeAccessor);
                    var pipelineInjector = new PipelineStepInjector(pipeline)
                        .OnSend(recurringCommandScopeInjector, PipelineRelativePosition.Before, typeof(AssignDefaultHeadersStep))
                        .OnSend(sendMessageTracker, PipelineRelativePosition.Before, typeof(SendOutgoingMessageStep))
                        .OnReceive(receiveMessageTracker, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));

                    if (pipelineAdditions != null)
                        return pipelineAdditions(pipelineInjector);

                    return pipelineInjector;
                });
            });
    }
}