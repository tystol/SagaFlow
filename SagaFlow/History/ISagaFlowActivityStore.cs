using System.Threading.Tasks;

namespace SagaFlow.History;

public interface ISagaFlowActivityStore
{
    Task<PagedResult<SagaFlowCommandStatus>> GetCommandHistory<T>(int pageIndex, int pageSize);
}