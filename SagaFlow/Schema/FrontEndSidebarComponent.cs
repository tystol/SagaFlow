namespace SagaFlow.Schema;

public class FrontEndSidebarComponent
{
    public FrontEndSidebarComponent(FrontEndPlugin plugin, string label, string icon, string pluginComponentId)
    {
        Plugin = plugin;
        Label = label;
        Icon = icon;
        PluginComponentId = pluginComponentId;
    }
    
    public FrontEndPlugin Plugin { get; }
    public string Label { get; }
    public string Icon { get; }
    public string PluginComponentId { get; }
    
}