using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SagaFlow.Schema;

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
                            .ToDictionary(rs => rs.PropertyInfo.Name.ToCamelCase(), rs => new ResourcePropertySchema
                            {
                                // TODO: allow an attribute to drive the display name of a resource provider property/column
                                Name = rs.PropertyInfo.Name,
                                Type = GetSchemaType(rs.PropertyInfo.PropertyType),
                                IsIdKey = rs.IsIdProperty,
                                IsTitleKey = rs.IsTitleProperty,
                            }),
                    }),
                Commands = schemaProvider.Commands
                    .ToDictionary(c => c.Id, c => new CommandDefinition
                    {
                        Name = c.Name,
                        Description = c.Description,
                        Href = c.RouteTemplate, // TODO: proper route resolution
                        Schema = c.Parameters
                            .ToDictionary(p => p.Id, p => new ParameterDefinition
                            {
                                Name = p.Name,
                                Description = p.Description,
                                Required = p.Required,
                                Type = GetSchemaType(p.InputType, p.ResourceProvider),
                                Multiselect = p.InputType != typeof(String) && p.InputType.GetEnumerableInnerType() != null,
                                ResourceListId = p.ResourceProvider?.Id
                            })
                    }),
                SidebarWidgets = new Dictionary<string, string>
                {
                    {"Example Widget", "/sagaflow/schema/example-widget"}
                }
            };
            return Task.FromResult(result);
        }
        
        [HttpGet]
        [Route("example-widget")]
        public Task<IActionResult> GetWidget()
        {
            var stream = System.IO.File.OpenRead("..\\SimpleMvcExample.Widgets\\example-widget\\dist\\example-widget.js");
            return Task.FromResult((IActionResult)File(stream, "text/javascript", true));
        }

        private static string GetSchemaType(Type type, ResourceProvider resourceProvider = null)
        {
            // TODO: better handling of multiselect
            if (type != typeof(String))
                type = type.GetEnumerableInnerType() ?? type;
            // TODO: better handling of value type fields that have an autocomplete via a resource provider
            if (type != typeof(String) && resourceProvider != null)
                return "relation";
            // TODO: mapping for .net types to front end conceptual types
            // TODO: map strong typed Ids to their primitive replacement for front end usage.
            return type.GetUnderlyingTypeIfNullable().Name.ToLowerInvariant();
        }
    }

    public class SchemaResponse
    {
        public IDictionary<string,ResourceListDefinition> ResourceLists { get; set; }
        public IDictionary<string,CommandDefinition> Commands { get; set; }
        public IDictionary<string,string> SidebarWidgets { get; set; }
    }

    public interface ISchemaDefinition<T> where T : IPropertySchema
    {
        public string Name { get; }
        public string Description { get; }
        public string Href { get; }
        public IDictionary<string,T> Schema { get; }
    }

    public class ResourceListDefinition : ISchemaDefinition<ResourcePropertySchema>
    {
        public string Name { get; set; }
        public string Description { get; set; } // TODO: populate descriptions for resource lists (tooltip in UI)
        public string Href { get; set; }
        public IDictionary<string,ResourcePropertySchema> Schema { get; set; }
    }

    public interface IPropertySchema
    {
        string Type { get; }
    }

    public class ResourcePropertySchema : IPropertySchema
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsIdKey { get; set; }
        public bool IsTitleKey { get; set; }
    }

    public class CommandDefinition : ISchemaDefinition<ParameterDefinition>
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string Href { get; init; }
        public IDictionary<string,ParameterDefinition> Schema { get; init; }
    }

    public class ParameterDefinition : IPropertySchema
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; } // TODO: proper type system to handle resource list inputs
        public bool Multiselect { get; set; }
        public string ResourceListId { get; set; }
    }
}
