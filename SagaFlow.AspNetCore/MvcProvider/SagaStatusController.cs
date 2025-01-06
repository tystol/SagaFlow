using Microsoft.AspNetCore.Mvc;
using SagaFlow.AspNetCore.Formatters;
using SagaFlow.History;

namespace SagaFlow.MvcProvider;

[ApiController]
[Route("[sagaflow-base-path]/sagas")]
public class SagaStatusController : ControllerBase
{
    private readonly ISagaFlowCommandStore _sagaFlowCommandStore;

    public SagaStatusController(
        ISagaFlowCommandStore sagaFlowCommandStore)
    {
        _sagaFlowCommandStore = sagaFlowCommandStore;
    }
    
    [HttpGet]
    public async Task<PagedResult<SagaFlowCommandStatus>> Get(int pageIndex = 0, int pageSize = 20, string? keyword = null)
    {
        return await _sagaFlowCommandStore.GetCommands(pageIndex, pageSize, keyword);
    }
}