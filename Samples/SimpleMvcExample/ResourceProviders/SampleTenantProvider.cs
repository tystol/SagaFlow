using System.ComponentModel;
using SagaFlow;

namespace SimpleMvcExample.ResourceProviders
{
    public class SampleTenantProvider : IResourceListProvider<SampleTenant, Guid>
    {
        private static List<SampleTenant> tenants = Enumerable.Range(0, 1000)
                .Select(t => new SampleTenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant " + (t + 1),
                })
                .ToList();

        public Task<IList<SampleTenant>> GetAll()
        {
            return Task.FromResult((IList<SampleTenant>)tenants);
        }
    }

    [DisplayName("Tenants")]
    public class SampleTenant : IResource<Guid>
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}
