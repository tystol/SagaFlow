using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SagaFlow.MvcProvider
{
    [ApiController]
    // Route attribute is required for ApiController's, but replaced at runtime
    // via MvcEndpointRouteConvention to resolve custom sagaflow parameters.
    [Route("[sagaflow-base-path]/resources/[resource-type]")]
    public class ResourceController<T> : ControllerBase
    {
        private readonly IResourceListProvider<T> resourceProvider;

        public ResourceController(IResourceListProvider<T> resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        [HttpGet]
        public Task<IList<T>> Get()
        {
            return resourceProvider.GetAll();
        }
    }
}
