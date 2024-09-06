using SagaFlow;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.ResourceProviders
{
    public class SampleDatabaseServerProvider : IResourceListProvider<DatabaseServer, DatabaseServerId>
    {
        private static readonly DateTime StartupTime = DateTime.UtcNow;
        
        private static readonly List<DatabaseServer> Databases = Enumerable.Range(0, 100)
                .Select(t => new DatabaseServer
                {
                    Id = new DatabaseServerId(String.Format("server-{0:00}",t+1)),
                    Name = "Server " + (t + 1),
                    CreatedUtc = StartupTime.Subtract(TimeSpan.FromDays(t + 5)).AddHours(t).AddMinutes(t*10),
                })
                .ToList();

        public Task<IEnumerable<DatabaseServer>> GetAll()
        {
            return Task.FromResult(Databases.AsEnumerable());
        }
    }

    public class DatabaseServer : IResource<DatabaseServerId>
    {
        public DatabaseServerId Id { get; init; }
        public string Name { get; init; }
        public DateTime CreatedUtc { get; set; }
    }
}
