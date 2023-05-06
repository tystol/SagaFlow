using System;

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
    }
}
