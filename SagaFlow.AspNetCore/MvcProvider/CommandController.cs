using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ISagaFlowActivityStore activityStore;
        private readonly ISagaFlowCommandBus commandBus;

        public CommandController(ISagaFlowActivityStore activityStore, ISagaFlowCommandBus commandBus)
        {
            this.activityStore = activityStore;
            this.commandBus = commandBus;
        }

        [HttpGet]
        [SagaFlowCommandJsonSerializer]
        public async Task<PaginatedResult<CommandHistory<T>>> Get(int page = 1, int pageSize = 50)
        {
            var history = await activityStore.GetCommandHistory<T>(page - 1, pageSize);
            return new PaginatedResult<CommandHistory<T>>
            {
                Items = history.Page.Select(c => new CommandHistory<T>
                {
                    Id = c.SagaFlowCommandId,
                    Status = c.Status,
                    Name = c.Name,
                    CommandName = c.CommandName,
                    CommandType = c.CommandType,
                    Command = (T) c.Command,
                    InitiatingUser = c.InitiatingUser,
                    StartDateTime = c.StartDateTime,
                    FinishDateTime = c.FinishDateTime,
                    Attempt = c.Attempt,
                    Progress = c.Progress,
                    LastError = c.LastError,
                    StackTrace = c.StackTrace,
                    Handlers = c.Handlers,
                    RelatedSagas = c.RelatedSagas,
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

    public record CommandHistory<T>
    {
        public SagaFlowCommandId Id { get; init; }

        public CommandStatus Status { get; init; } = CommandStatus.Sent;
    
        public string Name { get; init; }
    
        public string CommandName { get; init; }
    
        public string CommandType { get; init; }
    
        public T Command { get; init; }
    
        public string? InitiatingUser { get; init; }
    
        public DateTime StartDateTime { get; init; }
    
        public DateTime? FinishDateTime { get; set; }
    
        public int Attempt { get; set; }
    
        public double Progress { get; set; }

        public string? LastError { get; set; } = null;
        public string? StackTrace { get; set; } = null;
        public List<CommandHandlerStatusSummary> Handlers { get; set; }
        public List<SagaStatusSummary> RelatedSagas { get; set; }
    }
}
