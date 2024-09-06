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
    [Route("[sagaflow-base-path]/schema")]
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
                ResourceLists = schemaProvider.ResourceProviders
                    .ToDictionary(r => r.Id, r => new ResourceListDefinition
                    {
                        Name = r.Name,
                        Href = r.ListingRouteTemplate,
                        Schema = r.ResourceSchema
                            .Select(rs => new ResourcePropertySchema
                            {
                                Name = rs.PropertyInfo.Name.ToCamelCase(),
                                // TODO: map strong typed Ids to their primitive replacement for front end usage.
                                Type = rs.PropertyInfo.PropertyType.Name.ToLowerInvariant(),
                                IsIdKey = rs.IsIdProperty,
                                IsTitleKey = rs.IsTitleProperty,
                            }).ToList(),
                        ResourceMetadata = new ResourceMetadata
                        {
                            IdKey = "id",
                            TitleKey = "name",
                        }
                    }),
                Commands = schemaProvider.Commands
                    .ToDictionary(c => c.Id, c => new CommandDefinition
                    {
                        Name = c.Name,
                        Description = c.Description,
                        Href = c.RouteTemplate, // TODO: proper route resolution
                        Parameters = c.Parameters
                            .ToDictionary(p => p.Id, p => new ParameterDefinition
                            {
                                Name = p.Name,
                                Description = p.Description,
                                Required = true,
                                Type = p.InputType.Name, // TODO: mapping for .net types to front end conceptual types
                                ResourceListId = p.ResourceProvider?.Id
                            })
                    }),
            };
            return Task.FromResult(result);
        }
    }

    public class SchemaResponse
    {
        public IDictionary<string,ResourceListDefinition> ResourceLists { get; set; }
        public IDictionary<string,CommandDefinition> Commands { get; set; }
    }

    public class ResourceListDefinition
    {
        public string Name { get; set; }
        public string Href { get; set; }
        public List<ResourcePropertySchema> Schema { get; set; }
        public ResourceMetadata ResourceMetadata { get; set; } //TODO: remove, use ResourcePropertySchema instead
    }

    public class ResourcePropertySchema
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsIdKey { get; set; }
        public bool IsTitleKey { get; set; }
    }

    public class ResourceMetadata
    {
        public string IdKey { get; set; }
        public string TitleKey { get; set; }
    }

    public class CommandDefinition
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string Href { get; init; }
        public IDictionary<string,ParameterDefinition> Parameters { get; init; }
    }

    public class ParameterDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; } // TODO: proper type system to handle resource list inputs
        public string ResourceListId { get; set; }
    }
}
