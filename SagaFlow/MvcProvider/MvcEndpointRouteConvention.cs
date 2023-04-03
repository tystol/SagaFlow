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
                var commandKebab = commandType.Name.ToKebabCase();

                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);
                routeTemplate = routeTemplate.Replace("[command-type]", commandKebab);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));

                var command = module.Commands.FirstOrDefault(c => c.CommandType == commandType);
                command.RouteTemplate = routeTemplate;
            }

            if (controller.ControllerType.IsGenericType &&
                controller.ControllerType.GetGenericTypeDefinition() == typeof(ResourceController<>))
            {
                var resourceType = controller.ControllerType.GenericTypeArguments[0];
                // TODO: More accurate pluraliser
                var resourceKebab = resourceType.Name.ToKebabCase() + "s";

                var (routeSelector, routeTemplate) = GetRoute(controller);
                routeTemplate = routeTemplate.Replace("[sagaflow-base-path]", module.ApiBasePath);
                routeTemplate = routeTemplate.Replace("[resource-type]", resourceKebab);

                routeSelector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));

                var resourceDefinition = module.Resources.FirstOrDefault(r => r.Type == resourceType);
                resourceDefinition.ListingRouteTemplate = routeTemplate;
            }

            if (controller.ControllerType == typeof(SchemaController))
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
