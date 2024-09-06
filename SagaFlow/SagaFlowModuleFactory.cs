using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SagaFlow.Interfaces;
using SagaFlow.Schema;

namespace SagaFlow;

public static class SagaFlowModuleFactory
{
    private static readonly Regex CronMarkerRegex =
        new(@"(.*?)\s*\[cron:\s*(\S.+\S)\s*]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static SagaFlowModule Create(SagaFlowOptions options, string apiBasePath)
    {
        var resourceDefinitions = options.ResourceProviderTypes.SelectMany(f => f())
            .SelectMany(BuildResourceProvidersFromType)
            .ToList();

        // TODO: Better way of handling double up of IResourceListProvider<> vs IResourceListProvider<,> types.
        resourceDefinitions = resourceDefinitions
            .GroupBy(d => d.Id)
            .Select(g => g.OrderByDescending(d => d.IdType != null).First())
            .ToList();

        // If a resource type id is only used once (eg. strong typed Ids) then support automatically
        // mapping commands containing these Ids to resource providers.
        // TODO: If not unique, have an attribute based system to also provide this functionality.
        var typeBasedIdMap = resourceDefinitions
            .Where(rd => rd.IdType != null)
            // don't automap for primitive types as too high a chance of inadvertently matching normal properties
            .Where(rd => !new[]
            {
                typeof(int),
                typeof(string)
            }.Contains(rd.IdType))
            .GroupBy(rd => rd.IdType)
            .Select(g => new
            {
                IdType = g.Key,
                ResourceProviders = g.ToList()
            })
            .Where(g => g.ResourceProviders.Count == 1)
            .ToDictionary(g => g.IdType, g => g.ResourceProviders[0]);


        //var commandTypes = options.CommandTypes.SelectMany(f => f()).ToList();
        var commands = options.Commands.SelectMany(f => f())
            .Concat(options.CommandTypes.SelectMany(ct => ct().Select(t => BuildCommandFromType(t, typeBasedIdMap))))
            .ToList();
        var sagaFlowModule = new SagaFlowModule
        {
            ApiBasePath = apiBasePath,
            Commands = commands,
            ResourceProviders = resourceDefinitions,
            SageFlowStartup = options.HostHandlerSetup
        };
        return sagaFlowModule;
    }

    private static IEnumerable<ResourceProvider> BuildResourceProvidersFromType(Type type)
    {
        return new[]
            {
                typeof(IResourceListProvider<>),
                typeof(IResourceListProvider<,>)
            }
            .SelectMany(i => BuildResourceProviders(type, i));
    }

    private static IEnumerable<ResourceProvider> BuildResourceProviders(Type resourceProviderType,
        Type resourceProviderInterfaceType)
    {
        foreach (var resourceProviderInterface in resourceProviderType.GetInterfacesOfOpenGenericInterface(
                     resourceProviderInterfaceType))
        {
            var genericTypeParams = resourceProviderInterface.GetGenericArguments();
            var resourceType = genericTypeParams[0];
            // If using the multiple generic type param interface, then the 2nd type param is the type of Id used by the resource.
            var idType = genericTypeParams.Length > 1 ? genericTypeParams[1].GetUnderlyingTypeIfNullable() : null;

            var resourceTypeAttributes = resourceType.GetCustomAttributes();
            var displayNameAttribute = resourceTypeAttributes.OfType<DisplayNameAttribute>().FirstOrDefault();

            // TODO: Should probably use something else - DisplayName on a singular resource type DTO doesn't
            // quite fit, as the resource type represents a single, where as the below resource provider is more
            // a plural version.
            var resourceName = displayNameAttribute?.DisplayName ?? resourceType.Name + "s";

            var resourceProperties = resourceType.GetProperties();
            // TODO: allow a marker attribute to dictate the Id and Name/Title properties.
            var idProperty = resourceProperties.FirstOrDefault(p => p.Name == "Id");
            if (idProperty == null) throw new Exception("Resource Type is expected to have an Id property.");
            
            var nameProperty = resourceProperties.FirstOrDefault(p => p.Name == "Name");
            if (nameProperty == null) throw new Exception("Resource Type is expected to have a Name property.");

            yield return new ResourceProvider
            {
                Id = resourceName.ToKebabCase(),
                Type = resourceType,
                Name = resourceName,
                ProviderType = resourceProviderType,
                IdType = idType,
                ResourceSchema = resourceProperties
                    .Select(p => new ResourceSchema
                    {
                        PropertyInfo = p,
                        IsIdProperty = p == idProperty,
                        IsTitleProperty = p == nameProperty,
                    }).ToImmutableList(),
            };
        }
    }

    private static Command BuildCommandFromType(Type commandType,
        IDictionary<Type, ResourceProvider> resourceProviderMap)
    {
        var parameterProps = commandType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => new
            {
                PropertyInfo = p,
                Attributes = p.GetCustomAttributes()
            })
            //.Where(p => p.Attributes.Any(a => a is IgnoreAttribute)
            .ToList();

        var commandName = GetCommandName(commandType);
        var commandDescription = GetCommandDescription(commandType);
        var commandNameTemplate = GetCommandNameTemplate(commandType);
        var cronExpression = GetCronExpression(commandType);

        return new Command
        {
            Id = commandType.Name.ToKebabCase(),
            CommandType = commandType,
            Name = commandName,
            CommandNameTemplate = commandNameTemplate,
            Description = commandDescription,
            EventType = null,
            CronExpression = cronExpression,
            Parameters = parameterProps.Select(p => new CommandParameter
                {
                    Id = p.PropertyInfo.Name
                        .ToCamelCase(), // TODO: default to camelCase for property ids to match json / front end js conventions?
                    Name = p.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ??
                           p.PropertyInfo.Name,
                    Description = p.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description,
                    InputType = p.PropertyInfo.PropertyType,
                    ResourceProvider = GetTextSuggestionResourceProvider(p.PropertyInfo, resourceProviderMap) ?? 
                                       resourceProviderMap.GetValueOrDefault(p.PropertyInfo.PropertyType),
                    // TODO: Provide alternative to above to map resource providers to command properties. eg. attribute based.
                })
                .ToList()
        };
    }

   

    private static ResourceProvider GetTextSuggestionResourceProvider(PropertyInfo propertyInfo, IDictionary<Type, ResourceProvider> resourceProviderMap)
    {
        // only use Suggestion resource providers for string
        if (propertyInfo.PropertyType != typeof(string)) return null;

        var textSuggestionAttribute = propertyInfo.GetCustomAttribute<StringPropertySuggestionsAttribute>();

        return resourceProviderMap.Values.FirstOrDefault(
            provider =>
                provider.ProviderType == textSuggestionAttribute?.ResourceProviderType ||
                provider.ProviderType.FullName == textSuggestionAttribute?.ResourceProviderName ||
                provider.ProviderType.Name == textSuggestionAttribute?.ResourceProviderName);
    }
    
    private static string GetCommandName(Type commandType)
    {
        var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

        if (commandAttribute != null) return commandAttribute.Name;

        var displayNameAttribute = commandType.GetCustomAttribute<DisplayNameAttribute>();

        if (displayNameAttribute != null)
        {
            // In case the display name contains a cron expression in the form
            // Restore Database [cron: 0 0 * * Fri *]
            // Then we will resolve 'Restore Database' as the command name.
            var cronExpressionMatch = CronMarkerRegex.Match(displayNameAttribute.DisplayName);

            return cronExpressionMatch.Success
                ? cronExpressionMatch.Groups[1].Value
                : displayNameAttribute.DisplayName;
        }

        return commandType.Name;
    }
    
    private static string GetCommandDescription(Type commandType)
    {
        var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();
        if (commandAttribute != null && !string.IsNullOrEmpty(commandAttribute.Description))
            return commandAttribute.Description;
        
        var descriptionAttribute = commandType.GetCustomAttribute<DescriptionAttribute>();

        return descriptionAttribute?.Description;
    }
    
    private static string GetCommandNameTemplate(Type commandType)
    {
        var descriptionAttribute = commandType.GetCustomAttribute<CommandAttribute>();

        return descriptionAttribute?.NameTemplate;
    }

    private static string GetCronExpression(Type commandType)
    {
        var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

        if (commandAttribute != null && !string.IsNullOrEmpty(commandAttribute.Cron))
            return commandAttribute.Cron;

        var displayNameAttribute = commandType.GetCustomAttribute<DisplayNameAttribute>();

        if (displayNameAttribute != null)
        {
            // Attempts to resolve a cron expression from the Display Name attribute in the form
            // Restore Database [cron: 0 0 * * Fri *]
            var cronExpressionMatch = CronMarkerRegex.Match(displayNameAttribute.DisplayName);

            return cronExpressionMatch.Success
                ? cronExpressionMatch.Groups[2].Value
                : null;
        }

        return null;
    }
}