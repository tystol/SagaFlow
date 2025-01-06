using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;
using Rebus.Messages;
using SagaFlow.AspNetCore.Formatters;
using SagaFlow.History;

namespace SagaFlow.MvcProvider
{
    [ApiController]
    // Route attribute is required for ApiController's, but replaced at runtime
    // via MvcEndpointRouteConvention to resolve custom sagaflow parameters.
    [Route("[sagaflow-base-path]/commands/[command-type]")]
    public class CommandController<T> : ControllerBase where T : class, new()
    {
        private readonly SagaFlowModule module;
        private readonly ISagaFlowCommandStore sagaFlowCommandStore;
        private readonly ISagaFlowActivityStore activityStore;
        private readonly ISagaFlowCommandBus commandBus;

        public CommandController(SagaFlowModule module, ISagaFlowCommandStore sagaFlowCommandStore, ISagaFlowActivityStore activityStore, ISagaFlowCommandBus commandBus)
        {
            this.module = module;
            this.sagaFlowCommandStore = sagaFlowCommandStore;
            this.activityStore = activityStore;
            this.commandBus = commandBus;
        }

        [HttpGet]
        [SagaFlowCommandJsonSerializer]
        public async Task<PaginatedResult<CommandHistory<T>>> Get(int page = 1, int pageSize = 50)
        {
            var history = await sagaFlowCommandStore.GetCommandHistory<T>(page - 1, pageSize);
            return new PaginatedResult<CommandHistory<T>>
            {
                Items = history.Page.Select(c => new CommandHistory<T>
                {
                    Id = c.SagaFlowCommandId,
                    Status = c.Status,
                    Name = c.Name,
                    CommandName = c.CommandName,
                    CommandType = c.CommandType,
                    CommandArgs = c.CommandArgs,
                    Command = (T) c.Command, // TODO - send command snapshot
                    HumanReadableCommandPropertyValues = c.HumanReadableCommandPropertyValues,
                    InitiatingUser = c.InitiatingUser,
                    StartDateTime = c.StartDateTime,
                    FinishDateTime = c.FinishDateTime,
                    Attempt = c.Attempt,
                    Progress = c.Progress,
                    LastError = c.LastError,
                    StackTrace = c.StackTrace,
                }),
                Page = page,
                PageSize = pageSize,
                TotalItems = history.Total,
                // TODO: fix below calc
                TotalPages = history.Total / pageSize,
            };
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] T command)
        {
            await commandBus.Send(command);
            return Ok();
        }
    }

    public class CommandHistory<T>
    {
        public SagaFlowCommandId Id { get; init; }

        public CommandStatus Status { get; set; } = CommandStatus.Started;
    
        public string Name { get; init; }
    
        public string CommandName { get; init; }
    
        public string CommandType { get; init; }
    
        public string CommandArgs { get; init; }
        public T Command { get; init; }
    
        public ReadOnlyDictionary<string, string> HumanReadableCommandPropertyValues { get; init; }
    
        public string InitiatingUser { get; init; }
    
        public DateTime StartDateTime { get; init; }
    
        public DateTime? FinishDateTime { get; set; }
    
        public int Attempt { get; set; }
    
        public double Progress { get; set; }

        public string LastError { get; set; } = null;
        public string StackTrace { get; set; } = null;
    }
}
