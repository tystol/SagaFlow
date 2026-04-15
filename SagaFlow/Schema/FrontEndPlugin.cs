namespace SagaFlow.Schema;

public class FrontEndPlugin
{
    public FrontEndPlugin(string pluginId, string entryPointUrl)
    {
        PluginId = pluginId;
        EntryPointUrl = entryPointUrl;
    }

    public string PluginId { get; }
    public string EntryPointUrl { get; }
    
    internal SagaFlowModule SagaFlowModule { get; set; }

    internal void RegisterSidebarComponent(string label, string icon, string pluginComponentId)
    {
        SagaFlowModule.RegisterSidebarComponent(new FrontEndSidebarComponent(this, label, icon, pluginComponentId));
    }
}