using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace SagaFlow.MvcProvider
{
    public class MvcEndpointProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly SagaFlowModule sagaFlowModule;

        public MvcEndpointProvider(SagaFlowModule sagaFlowModule)
        {
            this.sagaFlowModule = sagaFlowModule;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var command in sagaFlowModule.Commands)
            {
                var c = typeof(CommandController<>).MakeGenericType(command.CommandType).GetTypeInfo();
                feature.Controllers.Add(c);
            }
            foreach (var r in sagaFlowModule.ResourceProviders)
            {
                var c = typeof(ResourceController<>).MakeGenericType(r.Type).GetTypeInfo();
                feature.Controllers.Add(c);
            }
        }
    }
}
