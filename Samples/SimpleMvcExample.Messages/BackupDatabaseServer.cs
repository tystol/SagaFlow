using System.ComponentModel;

namespace SimpleMvcExample.Messages
{
    [DisplayName("Backup Database Server")]
    public class BackupDatabaseServer : ICommand
    {
        [DisplayName("Database Server")]
        public DatabaseServerId? DatabaseServerId { get; init; }
        
        [DisplayName("Destination Filename")]
        [Description("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis vel turpis vel diam dignissim euismod ut non velit. Integer eget.")]
        public string? DestinationFilename { get; init; }
        
        [DisplayName("Override backup")]
        [Description("Override any existing backup sets")]
        public bool OverrideBackup { get; init; }
    }
}