using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace SimpleMvcExample.Controllers.Api;

public class ApiAuthorizationConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        // Check if the controller's route starts with /api
        if (controller.Attributes.Any(attr => 
                attr is ApiControllerAttribute ||
                attr is RouteAttribute route && route.Template.StartsWith("api")))
        {
            // Apply the Authorize filter to the controller
            controller.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
        }
        // // Alternatively, apply the Authorize attribute to all actions in the controller
        foreach (var action in controller.Actions)
        {
            if (action.Attributes.Any(attr => attr is RouteAttribute route && route.Template.StartsWith("api")))
            {
                action.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            }
        }
    }
}