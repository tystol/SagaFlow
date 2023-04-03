using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaFlow
{
    public interface IResourceListProvider<T>
    {
        Task<IList<T>> GetAll();
    }
}
