using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SagaFlow.Schema;

public record HumanReadableCommandPropertyValue(string Name, string Value);

public interface IHumanReadableCommandPropertiesResolver
{
    IDictionary<string, HumanReadableCommandPropertyValue>  GetDisplayablePropertyValues(object command);
}

internal class HumanReadableCommandPropertiesResolver : IHumanReadableCommandPropertiesResolver
{
    private readonly SagaFlowModule _sagaFlowModule;
    private readonly IServiceProvider _serviceProvider;

    public HumanReadableCommandPropertiesResolver(
        SagaFlowModule sagaFlowModule,
        IServiceProvider serviceProvider)
    {
        _sagaFlowModule = sagaFlowModule;
        _serviceProvider = serviceProvider;
    }
    
    public IDictionary<string, HumanReadableCommandPropertyValue> GetDisplayablePropertyValues(object command)
    {
        var commandType = command.GetType();
        var displayableProperties = new Dictionary<string, HumanReadableCommandPropertyValue>();
        
        var commandDefinition = _sagaFlowModule.Commands.FirstOrDefault(c => c.CommandType == commandType)
                                ?? throw new InvalidOperationException(
                                    $"Unknown command of type {commandType.FullName}");
        
        var commandProperties = commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var resourceProviderByParameterName = GetResourceProviderByParameterName(commandDefinition);

        foreach (var propertyInfo in commandProperties)
        {
            // Look for the Parameter in our command definition
            var parameter = commandDefinition.Parameters.FirstOrDefault(p => p.Id == propertyInfo.Name.ToCamelCase());
            if (parameter == null) continue;
            
            // Get the value of the property in our command
            var value = propertyInfo.GetValue(command);
            
            // If there is an associated Resource provider
            if (parameter.ResourceProvider != null &&
                resourceProviderByParameterName.TryGetValue(parameter.ResourceProvider.Id, out var resources))
            {
                // Get a name of the associated resource to use as the value
                value = resources.Cast<dynamic>().FirstOrDefault(r => r.Id.Equals(value))?.Name ?? value;
            }

            displayableProperties[parameter.Id] = new HumanReadableCommandPropertyValue(
                parameter.Name,
                value?.ToString());
        }

        return displayableProperties;
    }
    
    private IDictionary<string, IEnumerable> GetResourceProviderByParameterName(Command commandDefinition)
    {
        var resourceDictionary = new Dictionary<string, IEnumerable>();
        
        foreach (var parameter in commandDefinition.Parameters)
        {
            if (parameter.ResourceProvider is null) continue;
            
            if (resourceDictionary.Keys.Contains(parameter.ResourceProvider.Id)) continue;

            var resourceProviderType =
                typeof(IResourceListProvider<>).MakeGenericType(parameter.ResourceProvider.Type);
            var resourceProvider = _serviceProvider.GetService(resourceProviderType);

            var getAllMethod =
                parameter.ResourceProvider.ProviderType.GetMethod(nameof(IResourceListProvider<object>.GetAll))
                ?? throw new InvalidOperationException("Expected member GetAll()");

            var resourceTask = (dynamic)getAllMethod.Invoke(resourceProvider, Array.Empty<object>())
                               ?? throw new InvalidOperationException("Expected resources to return)");
            
            var resources = resourceTask.Result;
            
            resourceDictionary.Add(parameter.ResourceProvider.Id, resources);
        }

        return resourceDictionary;
    }
}