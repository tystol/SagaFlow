using SagaFlow;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.ResourceProviders
{
    public class SampleDatabaseServerProvider : IResourceListProvider<DatabaseServer, DatabaseServerId>
    {
        private static List<DatabaseServer> databases = Enumerable.Range(0, 10)
                .Select(t => new DatabaseServer
                {
                    Id = new DatabaseServerId(String.Format("server-{0:00}",t+1)),
                    Name = "Server " + (t + 1),
                })
                .ToList();

        public Task<IList<DatabaseServer>> GetAll()
        {
            return Task.FromResult((IList<DatabaseServer>)databases);
        }
    }

    public class DatabaseServer : IResource<DatabaseServerId>
    {
        public DatabaseServerId Id { get; init; }
        public string Name { get; init; }
    }
}
