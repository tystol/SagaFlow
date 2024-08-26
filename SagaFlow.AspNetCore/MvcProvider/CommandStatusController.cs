using Microsoft.AspNetCore.Mvc;
using SagaFlow.AspNetCore.Formatters;
using SagaFlow.History;

namespace SagaFlow.MvcProvider;

[ApiController]
[Route("[sagaflow-base-path]/commands")]
[SagaFlowCommandJsonSerializer]
public class CommandStatusController : ControllerBase
{
    private readonly ISagaFlowActivityStore activityStore;

    public CommandStatusController(ISagaFlowActivityStore activityStore)
    {
        this.activityStore = activityStore;
    }
    
    [HttpGet]
    public Task<PagedResult<SagaFlowCommandStatus>> Get(int pageIndex = 0, int pageSize = 20, string? keyword = null)
    {
        throw new NotImplementedException();
    }
}