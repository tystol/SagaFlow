using SagaFlow;

namespace SimpleMvcExample.ResourceProviders;

public class JobsResourceProvider:IResourceListProvider<Job, Guid>
{
    public Task<IEnumerable<Job>> GetAll()
    {
        return Task.FromResult((IEnumerable<Job>)new List<Job>
        {
            new Job {Id = Guid.NewGuid(), Name = "Backup Database"},
            new Job {Id = Guid.NewGuid(), Name = "Restore Database"},
            new Job {Id = Guid.NewGuid(), Name = "Deploy Application"},
            new Job {Id = Guid.NewGuid(), Name = "Run Unit Tests"},
            new Job {Id = Guid.NewGuid(), Name = "Monitor System Health"},
            new Job {Id = Guid.NewGuid(), Name = "Update Dependencies"},
            new Job {Id = Guid.NewGuid(), Name = "Generate Reports"},
            new Job {Id = Guid.NewGuid(), Name = "Clean Up Logs"},
            new Job {Id = Guid.NewGuid(), Name = "Optimize Database"},
            new Job {Id = Guid.NewGuid(), Name = "Configure Load Balancer"}
        });
    }
}

public class Job : IResource<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}