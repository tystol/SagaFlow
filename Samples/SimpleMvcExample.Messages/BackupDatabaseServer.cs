namespace SimpleMvcExample.Messages
{
    public class BackupDatabaseServer : ICommand
    {
        public DatabaseServerId? DatabaseServerId { get; init; }
        public string? DestinationFilename { get; init; }
    }
}