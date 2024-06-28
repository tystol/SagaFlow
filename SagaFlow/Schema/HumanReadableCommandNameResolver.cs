using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SagaFlow.Schema;

/// <summary>
/// Resolves a human readable name for an issued SagaFlow command
/// </summary>
public interface IHumanReadableCommandNameResolver
{
    string ResolveCommandName(object command);
}

internal class HumanReadableCommandNameResolver : IHumanReadableCommandNameResolver
{
    private readonly IHumanReadableCommandPropertiesResolver _propertiesResolver;
    private readonly SagaFlowModule _sagaFlowModule;
    private readonly IServiceProvider _serviceProvider;

    private static readonly Regex TemplateRegex = new Regex("[^{}]*{([^{}]+)}[^{}]*");

    public HumanReadableCommandNameResolver(
        IHumanReadableCommandPropertiesResolver propertiesResolver,
        SagaFlowModule sagaFlowModule,
        IServiceProvider serviceProvider)
    {
        _propertiesResolver = propertiesResolver;
        _sagaFlowModule = sagaFlowModule;
        _serviceProvider = serviceProvider;
    }
    
    public string ResolveCommandName(object command)
    {
        var commandType = command.GetType();
        var commandDefinition = _sagaFlowModule.Commands.FirstOrDefault(c => c.CommandType == commandType)
                                ?? throw new InvalidOperationException(
                                    $"Unknown command of type {commandType.FullName}");

        if (string.IsNullOrWhiteSpace(commandDefinition.CommandNameTemplate))
            return commandDefinition.Name.ToTitleCase();
        
        // Given a template in the form
        // Hello, my name is {Name} and my favourite this to eat is {FavouriteFood}.
        var name = commandDefinition.CommandNameTemplate;
        
        // Get a dictionary of property id to their displayable human readable values
        var displayableValues = _propertiesResolver.GetDisplayablePropertyValues(command);
        
        // Use the regex to get the template keys: Name and FavouriteFood
        // which should be the name of Properties in the incoming command
        var matches = TemplateRegex.Matches(commandDefinition.CommandNameTemplate);
        
        foreach (Match match in matches)
        {
            // Get the template key, which should be a name of a property in our command eg. Name or FavouriteFood
            var templateKey = match.Groups[1].Value;

            // Get a value from the template key
            var value = displayableValues[templateKey.ToCamelCase()];
            
            // Replace the template placeholder with the displayable value.
            name = name.Replace($"{{{templateKey}}}", value.Value);
        }

        return name;
    }
}