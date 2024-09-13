using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using SagaFlow.Authentications;
using SagaFlow.Schema;

namespace SagaFlow.History;

public static class SagaFlowRebusEvents
{
    internal const string SagaFlowCommandId = nameof(SagaFlowCommandId);

    internal const string SagaFlowInitiatingUsername = nameof(SagaFlowInitiatingUsername);
    
    internal const string SagaFlowCommandName = nameof(SagaFlowCommandName);
    
    internal const string SagaFlowCommandStartDate = nameof(SagaFlowCommandStartDate);
    
    internal const string SagaFlowCommandAttemptCounter = nameof(SagaFlowCommandAttemptCounter);
    
    internal static MessageSentEventHandler OnBeforeMessageSent(
        SagaFlowModule sagaFlowModule)
    {
        return (_, headers, command, _) =>
        {
            using var scope = sagaFlowModule.ServiceProvider.CreateScope();
            var usernameProvider = scope.ServiceProvider.GetRequiredService<IUsernameProvider>();
            var commandNameResolver = scope.ServiceProvider.GetRequiredService<IHumanReadableCommandNameResolver>();
            
            // Generate a new and unique SagaFlowCommandId and assign it to the context
            headers.TryAdd(SagaFlowCommandId, new SagaFlowCommandId());
            
            headers.TryAdd(SagaFlowCommandStartDate, DateTime.UtcNow.ToString("o"));

            headers.TryAdd(SagaFlowInitiatingUsername, usernameProvider.CurrentUsername);
            headers.TryAdd(SagaFlowCommandName, commandNameResolver.ResolveCommandName(command));
            
           

            if (!headers.TryAdd(SagaFlowCommandAttemptCounter, "1"))
            {
                if (int.TryParse(headers[SagaFlowCommandAttemptCounter], out var currentAttempt))
                {
                    headers[SagaFlowCommandAttemptCounter] = (++currentAttempt).ToString();
                }
            }
        };
    }
    
    internal static MessageHandledEventHandler OnBeforeMessageHandled(SagaFlowModule sagaFlowModule)
    {
        return (_, headers, command, _, _) =>
        {
            SagaFlowCommandId sagaFlowCommandId = headers[SagaFlowCommandId]; 
            var commandDefinition = sagaFlowModule.Commands.FirstOrDefault(c => c.CommandType == command.GetType());

            if (commandDefinition != null)
            {
                using var scope = sagaFlowModule.ServiceProvider.CreateScope();
                
                var sagaFlowCommandStore = scope.ServiceProvider.GetRequiredService<ISagaFlowCommandStore>();
                var humanReadableCommandPropertiesResolver =
                    scope.ServiceProvider.GetRequiredService<IHumanReadableCommandPropertiesResolver>();

                var humanReadablePropertyValues =
                    humanReadableCommandPropertiesResolver.GetDisplayablePropertyValues(command);

                if (!int.TryParse(headers[SagaFlowCommandAttemptCounter], out var attempt))
                    attempt = 1;

                if (!DateTime.TryParse(headers[SagaFlowCommandStartDate], out var startDate))
                    startDate = DateTime.UtcNow;

                var commandStatus =
                    // Try to get an existing Command from the store
                    sagaFlowCommandStore.GetCommand(sagaFlowCommandId).Result ??
                    // or create a new one if it doesn't exist
                    new SagaFlowCommandStatus()
                    {
                        SagaFlowCommandId = sagaFlowCommandId,
                        Name = headers[SagaFlowCommandName],
                        CommandName = commandDefinition.Name,
                        CommandType = commandDefinition.CommandType.Name,
                        Command = command,
                        InitiatingUser = headers[SagaFlowInitiatingUsername],
                        StartDateTime = startDate,
                        CommandArgs = JsonSerializer.Serialize(command),
                        HumanReadableCommandPropertyValues = new ReadOnlyDictionary<string, string>(humanReadablePropertyValues.Values
                            .ToDictionary(
                                item => (string)item.Name,
                                item => (string)item.Value))
                    };

                commandStatus.Attempt = attempt;
                commandStatus.Progress = 0;
                
                sagaFlowCommandStore.AddOrUpdateCommand(commandStatus).Wait();
                
                PublishSagaFlowCommandStateChanged(commandStatus, scope.ServiceProvider).Wait();
            }
        };
    }
    
    internal static MessageHandledEventHandler OnAfterMessageHandled(
        SagaFlowModule sagaFlowModule)
    {
        return (_, headers, _, context, _) =>
        {
            using var scope = sagaFlowModule.ServiceProvider.CreateScope();
            var sagaFlowCommandService = scope.ServiceProvider.GetRequiredService<ISagaFlowCommandStatusService>();
            
            SagaFlowCommandId sagaFlowCommandId = headers[SagaFlowCommandId];
            var exception = context.Load<Exception>();

            SagaFlowCommandStatus commandStatus;

            if (exception != null)
            {
                commandStatus = sagaFlowCommandService.UpdateErrored(sagaFlowCommandId, exception).Result;

                PublishErroredCommandStatus(commandStatus, scope.ServiceProvider).Wait();
            }
            else
            {
                commandStatus = sagaFlowCommandService.UpdateProgress(sagaFlowCommandId, 100).Result;
                
                PublishSuccessCommandStatus(commandStatus, scope.ServiceProvider).Wait();
            }
            
            PublishSagaFlowCommandStateChanged(commandStatus, scope.ServiceProvider).Wait();
        };
    }

    private static async Task PublishSuccessCommandStatus(SagaFlowCommandStatus commandStatus, IServiceProvider serviceProvider)
    {
        var updateEvents = serviceProvider.GetServices<ISagaFlowCommandSucceededHandler>();

        await Task.WhenAll(
            updateEvents.Select(handler => handler.HandleSagaFlowCommandSucceeded(commandStatus)));
    }
    
    private static async Task PublishErroredCommandStatus(SagaFlowCommandStatus commandStatus, IServiceProvider serviceProvider)
    {
        var updateEvents = serviceProvider.GetServices<ISagaFlowCommandErroredHandler>();

        await Task.WhenAll(
            updateEvents.Select(handler => handler.HandleSagaFlowCommandErrored(commandStatus)));
    }

    private static async Task PublishSagaFlowCommandStateChanged(SagaFlowCommandStatus commandStatus, IServiceProvider serviceProvider)
    {
        var updateEvents = serviceProvider.GetServices<ISagaFlowCommandStateChangedHandler>();

        await Task.WhenAll(
            updateEvents.Select(handler => handler.HandleSagaFlowCommandStatusUpdate(commandStatus)));
    }
}