﻿using Microsoft.AspNetCore.Mvc;
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
                        Description = null, // TODO: Add descriptions for resource lists?
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
            };
            return Task.FromResult(result);
        }

        private static string GetSchemaType(Type type, ResourceProvider? resourceProvider = null)
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
        public required string Name { get; init; }
        public required string? Description { get; init; } // TODO: populate descriptions for resource lists (tooltip in UI)
        public required string Href { get; init; }
        public required IDictionary<string,ResourcePropertySchema> Schema { get; init; }
    }

    public interface IPropertySchema
    {
        string Type { get; }
    }

    public class ResourcePropertySchema : IPropertySchema
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required bool IsIdKey { get; init; }
        public required bool IsTitleKey { get; init; }
    }

    public class CommandDefinition : ISchemaDefinition<ParameterDefinition>
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Href { get; init; }
        public required IDictionary<string,ParameterDefinition> Schema { get; init; }
    }

    public class ParameterDefinition : IPropertySchema
    {
        public required string Name { get; init; }
        public required string Description { get; set; }
        public required bool Required { get; init; }
        public required string Type { get; init; } // TODO: proper type system to handle resource list inputs
        public required bool Multiselect { get; init; }
        public required string? ResourceListId { get; init; }
    }
}
