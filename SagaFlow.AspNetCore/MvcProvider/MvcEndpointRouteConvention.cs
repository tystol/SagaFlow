using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SagaFlow.MvcProvider
{
    public class MvcEndpointRouteConvention : IControllerModelConvention
    {
        private readonly SagaFlowModule module;

        public MvcEndpointRouteConvention(SagaFlowModule module)
        {
            this.module = module;
        }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType &&
                controller.ControllerType.GetGenericTypeDefinition() == typeof(CommandController<>))
            {
                var commandType = controller.ControllerType.GenericTypeArguments[0];
                // TODO: proper way of resolving command definition from here without inferring from type system.
                var command = module.Commands.FirstOrDefault(c => c.CommandType == commandType);

                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);
                routeTemplate = routeTemplate.Replace("[command-type]", command.Id);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));
                command.RouteTemplate = routeTemplate;
            }

            if (controller.ControllerType.IsGenericType &&
                controller.ControllerType.GetGenericTypeDefinition() == typeof(ResourceController<>))
            {
                var resourceType = controller.ControllerType.GenericTypeArguments[0];
                // TODO: proper way of resolving resource definition from here without inferring from type system.
                // eg. Should allow for more than 1 resource provider of the same resource type.
                var resourceDefinition = module.ResourceProviders.FirstOrDefault(r => r.Type == resourceType);

                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);
                routeTemplate = routeTemplate.Replace("[resource-type]", resourceDefinition.Id);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));
                resourceDefinition.ListingRouteTemplate = routeTemplate;
            }

            if (controller.ControllerType == typeof(SchemaController))
            {
                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));
            }
            
            if (controller.ControllerType == typeof(CommandStatusController))
            {
                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));
            }
        }

        private static (SelectorModel routeSelector, string routeTemplate) GetRoute(ControllerModel controller)
        {
            var routeSelector = controller.Selectors
                    .Where(s => s.AttributeRouteModel != null)
                    .FirstOrDefault();
            var routeTemplate = routeSelector.AttributeRouteModel.Template;

            return (routeSelector, routeTemplate);
        }
    }
}
