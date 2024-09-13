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
        private readonly IPaginatedProvider<T> paginatedProvider;

        public ResourceController(IResourceListProvider<T> resourceProvider)
        {
            this.resourceProvider = resourceProvider;
            paginatedProvider = this.resourceProvider as IPaginatedProvider<T>;
        }

        [HttpGet]
        public async Task<PaginatedResult<T>> Get(int? page, int? pageSize)
        {
            if (paginatedProvider != null && page != null && pageSize != null)
            {
                var pageResult = await paginatedProvider.GetPage(page.Value - 1, pageSize.Value);
                return new PaginatedResult<T>
                {
                    Items = pageResult.Resources,
                    Page = page.Value,
                    PageSize = pageSize.Value,
                    TotalItems = pageResult.TotalResources,
                    // TODO: some simple tests to verify TotalPages correct in all cases, below code is not 100%.
                    TotalPages = pageResult.TotalResources / pageSize,
                };
            }

            return new PaginatedResult<T>
            {
                Page = 1,
                PageSize = -1,
                Items = await resourceProvider.GetAll(),
            };
        }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int? TotalItems { get; set; }
        public int? TotalPages { get; set; }
    }
}
