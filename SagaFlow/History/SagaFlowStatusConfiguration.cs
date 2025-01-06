using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline.Send;
using Rebus.Sagas;

namespace SagaFlow.History;

internal static class SagaFlowStatusConfiguration
{
    internal static RebusConfigurer ConfigureSagaFlowEventsForRebus(this RebusConfigurer configure, SagaFlowModule sagaFlowModule)
    {
        return configure
            .Events(events =>
            {
                events.BeforeMessageSent += SagaFlowRebusEvents.OnBeforeMessageSent(sagaFlowModule);
                events.BeforeMessageHandled += SagaFlowRebusEvents.OnBeforeMessageHandled(sagaFlowModule);
                events.AfterMessageSent += SagaFlowRebusEvents.OnAfterMessageSent(sagaFlowModule);
                events.AfterMessageHandled += SagaFlowRebusEvents.OnAfterMessageHandled(sagaFlowModule);
            })
            .Options(opts =>
            {
                opts.LogPipeline(verbose:true);
                opts.Decorate<IPipeline>(c =>
                {
                    var sagaFlowActivityStore = sagaFlowModule.ServiceProvider.GetService<ISagaFlowActivityStore>() ??
                        throw new InvalidOperationException("Could not resolve ISagaFlowActivityStore");
                    var pipeline = c.Get<IPipeline>();
                    var sendStep = new SagaFlowOutgoingMessageTracker(sagaFlowActivityStore);
                    var receiveStep = new SagaFlowIncomingMessageTracker(sagaFlowActivityStore);
                    return new PipelineStepInjector(pipeline)
                        .OnSend(sendStep, PipelineRelativePosition.Before, typeof(SendOutgoingMessageStep))
                        .OnReceive(receiveStep, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));
                });
            });
    }
}