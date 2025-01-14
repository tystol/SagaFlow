using Microsoft.AspNetCore.Mvc;
using SagaFlow.History;

namespace SagaFlow.MvcProvider;

[ApiController]
[Route("[sagaflow-base-path]/sagas")]
public class SagaStatusController : ControllerBase
{
    private readonly ISagaFlowActivityStore activityStore;

    public SagaStatusController(ISagaFlowActivityStore activityStore)
    {
        this.activityStore = activityStore;
    }
    
    [HttpGet("{id}")]
    public async Task<PagedResult<SagaFlowCommandStatus>> Get(string id)
    {
        throw new NotImplementedException();
    }
}