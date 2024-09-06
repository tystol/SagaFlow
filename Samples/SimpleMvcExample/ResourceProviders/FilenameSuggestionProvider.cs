using SagaFlow;
using SimpleMvcExample.Messages.StrongTyping;

namespace SimpleMvcExample.ResourceProviders;

public class FilenameSuggestionProvider : IResourceListProvider<Filename, FilenameId>
{
    public Task<IEnumerable<Filename>> GetAll()
    {
        return Task.FromResult<IEnumerable<Filename>>(
            new List<Filename>()
            {
                new Filename(new FilenameId(1), "backup.bak"),
                new Filename(new FilenameId(2), "test.bak"),
                new Filename(new FilenameId(3), "snapshot.bak"),
            });
    }
}

public record FilenameId(int Value) : StronglyTypedId<int>(Value);

public record Filename(FilenameId Id, string Name) : IResource<FilenameId>;