using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaFlow;

namespace SimpleMvcExample.ResourceProviders
{
    public class SampleTenantProvider : IResourceListProvider<Tenant>
    {
        private static List<Tenant> tenants = Enumerable.Range(0, 1000)
                .Select(t => new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant " + (t + 1),
                })
                .ToList();


        public Task<IList<Tenant>> GetAll()
        {
            return Task.FromResult((IList<Tenant>)tenants);
        }
    }

    public class Tenant
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}
