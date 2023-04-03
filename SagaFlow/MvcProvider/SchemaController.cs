using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SagaFlow.MvcProvider
{
    [ApiController]
    // Route attribute is required for ApiController's, but replaced at runtime
    // via MvcEndpointRouteConvention to resolve custom sagaflow parameters.
    [Route("[sagaflow-base-path]")]
    public class SchemaController : ControllerBase
    {
        private readonly ISagaFlowSchemaProvider schemaProvider;

        public SchemaController(ISagaFlowSchemaProvider schemaProvider)
        {
            this.schemaProvider = schemaProvider;
        }

        [HttpGet]
        public Task<SchemaResponse> Get()
        {
            var result = new SchemaResponse
            {
                ResourceLists = schemaProvider.Resources
                    .Select(r => new ResourceList
                    {
                        DisplayName = r.DisplayName,
                        Href = r.ListingRouteTemplate,
                    })
                    .ToList(),
                Commands = schemaProvider.Commands
                    .ToDictionary(c => c.Name.ToKebabCase(), c => new CommandDefinition
                    {
                        DisplayName = c.DisplayName,
                        Href = c.RouteTemplate, // TODO: proper route resolution
                        Parameters = c.Parameters
                            .ToDictionary(p => p.Name.ToKebabCase(), p => new Parameter
                            {
                                Description = p.Description,
                                Required = true,
                                Type = p.InputType.Name,
                            })
                    }),
            };
            return Task.FromResult(result);
        }
    }

    public class SchemaResponse
    {
        public List<ResourceList> ResourceLists { get; set; }
        public IDictionary<string,CommandDefinition> Commands { get; set; }
    }

    public class ResourceList
    {
        public string DisplayName { get; set; }
        public string Href { get; set; }
    }

    public class CommandDefinition
    {
        public string DisplayName { get; set; }
        public string Href { get; set; }
        public IDictionary<string,Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; } // TODO: proper type system to handle resource list inputs
    }
}
