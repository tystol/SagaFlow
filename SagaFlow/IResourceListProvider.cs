using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaFlow
{
    public interface IResourceListProvider<TResource>
    {
        Task<IEnumerable<TResource>> GetAll();
    }
    
    public interface IResourceListProvider<TResource,TId> : IResourceListProvider<TResource> where TResource : IResource<TId>
    {
    }

    public interface IPaginatedProvider<TResource>
    {
        Task<PaginatedResult<TResource>> GetPage(int page, int pageSize);
    }

    public class PaginatedResult<TResource>
    {
        public IEnumerable<TResource> Resources { get; set; }
        public int? TotalResources { get; set; }
    }

    public interface IResource<out TId>
    {
        public TId Id { get; }
        
        public string Name { get; }
    }
}
