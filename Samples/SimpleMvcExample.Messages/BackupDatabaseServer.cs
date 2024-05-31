using System.ComponentModel;

namespace SimpleMvcExample.Messages
{
    public class BackupDatabaseServer : ICommand
    {
        [DisplayName("Database Server")]
        public DatabaseServerId? DatabaseServerId { get; init; }
        
        [DisplayName("Destination Filename")]
        public string? DestinationFilename { get; init; }
    }
}