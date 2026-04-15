using System;
using System.Collections.Generic;
using SagaFlow.Schema;

namespace SagaFlow
{
    public class SagaFlowModule : ISagaFlowSchemaProvider
    {
        private readonly Dictionary<string, FrontEndPlugin> frontEndPlugins = new Dictionary<string, FrontEndPlugin>();
        private readonly List<FrontEndSidebarComponent> frontEndSidebarComponents = new List<FrontEndSidebarComponent>();
        public string ApiBasePath { get; init; }
        public IReadOnlyList<Command> Commands { get; init; }
        public IReadOnlyList<ResourceProvider> ResourceProviders { get; init; }
        public IReadOnlyDictionary<string,FrontEndPlugin> FrontEndPlugins => frontEndPlugins;
        public IReadOnlyList<FrontEndSidebarComponent> FrontEndSidebarComponents => frontEndSidebarComponents;
        internal IServiceProvider ServiceProvider { get; set; }

        internal List<Action<object>> SageFlowStartup = new List<Action<object>>();
        
        internal void RegisterFrontEndPlugin(FrontEndPlugin plugin)
        {
            plugin.SagaFlowModule = this;
            frontEndPlugins.Add(plugin.PluginId, plugin);
        }
        
        internal void RegisterSidebarComponent(FrontEndSidebarComponent component)
        {
            frontEndSidebarComponents.Add(component);
        }
    }
}
