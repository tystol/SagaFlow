using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaFlow
{
    public interface IResourceListProvider<T>
    {
        Task<IList<T>> GetAll();
    }
    
    public interface IResourceListProvider<TResource,TId> : IResourceListProvider<TResource> where TResource : IResource<TId>
    {
    }

    public interface IResource<out TId>
    {
        public TId Id { get; }
        
        public string Name { get; }
    }
}
