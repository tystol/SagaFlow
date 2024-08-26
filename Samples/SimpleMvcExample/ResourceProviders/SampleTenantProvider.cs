using System.ComponentModel;
using SagaFlow;

namespace SimpleMvcExample.ResourceProviders
{
    public class SampleTenantProvider : IResourceListProvider<SampleTenant, Guid>, IPaginatedProvider<SampleTenant>
    {
        private static readonly List<SampleTenant> Tenants = Enumerable.Range(0, 1000)
                .Select(t => new SampleTenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Tenant " + (t + 1),
                    Description = "Lorem Ipsum...",
                    LastLoginTime = DateTime.Now.AddSeconds(-1*Random.Shared.Next(0,36000)),
                })
                .ToList();

        public Task<IEnumerable<SampleTenant>> GetAll()
        {
            return Task.FromResult(Tenants.AsEnumerable());
        }

        public Task<PaginatedResult<SampleTenant>> GetPage(int page, int pageSize)
        {
            return Task.FromResult(new PaginatedResult<SampleTenant>
            {
                Resources = Tenants.Skip(page * pageSize).Take(pageSize),
                TotalResources = Tenants.Count,
            });
        }
    }

    [DisplayName("Tenants")]
    public class SampleTenant : IResource<Guid>
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime LastLoginTime { get; set; }
    }
}
