using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaFlow.Status;

public interface ISagaFlowCommandStore
{
    Task AddOrUpdateCommand(SagaFlowCommandStatus commandStatus);

    Task<SagaFlowCommandStatus> GetCommand(SagaFlowCommandId commandId);

    Task<PagedResult<SagaFlowCommandStatus>> GetCommands(int index, int pageSize, string keyword);
}

/// <summary>
/// Default SagaFlow command store, stores queued commands in-memory
/// </summary>
internal class InMemorySagaFlowCommandStore : ISagaFlowCommandStore
{
    private const int MaxNumberOfItems = 300;
    private static readonly IDictionary<SagaFlowCommandId, SagaFlowCommandStatus> InMemoryStore =
        new ConcurrentDictionary<SagaFlowCommandId, SagaFlowCommandStatus>();
    
    public Task AddOrUpdateCommand(SagaFlowCommandStatus commandStatus)
    {
        InMemoryStore[commandStatus.SagaFlowCommandId] = commandStatus;

        EnsureMaxNumberOfItems();
        
        return Task.CompletedTask;
    }

    public Task<SagaFlowCommandStatus> GetCommand(SagaFlowCommandId sagaFlowCommandId)
    {
        return Task.FromResult(InMemoryStore.GetValueOrDefault(sagaFlowCommandId));
    }

    public Task<PagedResult<SagaFlowCommandStatus>> GetCommands(int index, int pageSize, string keyword)
    {
        var query = InMemoryStore.Values
            .Where(command =>
                string.IsNullOrWhiteSpace(keyword) ||
                command.CommandName.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                command.Name.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                command.InitiatingUser.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                command.HumanReadableCommandPropertyValues.Values.Any(value => value?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) == true) ||
                command.LastError?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) == true ||
                command.StackTrace?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) == true);
        
        var total = query.Count();
        var items = query
            .OrderByDescending(command => command.StartDateTime)
            .Skip(pageSize * index)
            .Take(pageSize);

        return Task.FromResult(new PagedResult<SagaFlowCommandStatus>(
            items,
            index,
            pageSize,
            total));
    }
    
    /// <summary>
    /// So the in-memory doesn't grow forever, once we hit the max limit we will remove the oldest item from order
    /// they were added to the store.
    /// </summary>
    private static void EnsureMaxNumberOfItems()
    {
        if (InMemoryStore.Values.Count > MaxNumberOfItems)
        {
            var oldestItems = InMemoryStore
                .OrderBy(item => item.Value.StartDateTime)
                .Take(InMemoryStore.Values.Count - MaxNumberOfItems).ToArray();

            foreach (var item in oldestItems)
            {
                InMemoryStore.Remove(item);
            }
        }
    }
}