using System;
using System.Collections.Generic;
using System.Reflection;

namespace SagaFlow.Schema
{
    public class ResourceProvider
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
        public Type IdType { get; internal set; }
        public Type ProviderType { get; internal set; }
        public string ListingRouteTemplate { get; internal set; }
        
        public IReadOnlyList<ResourceSchema> ResourceSchema { get; internal set; }
    }

    public class ResourceSchema
    {
        public PropertyInfo PropertyInfo { get; internal set; }
        public bool IsIdProperty { get; internal set; }
        public bool IsTitleProperty { get; internal set; }
    }
}
