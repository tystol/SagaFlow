using System;

namespace SagaFlow.Schema
{
    public class Resource
    {
        public string DisplayName { get; internal set; }
        public Type Type { get; internal set; }
        public Type IdType { get; internal set; }
        public Type ProviderType { get; internal set; }
        public string ListingRouteTemplate { get; internal set; }
    }
}
