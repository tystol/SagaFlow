using System;
using System.ComponentModel;
using SagaFlow.Interfaces;

namespace SimpleMvcExample.Messages
{
    [Command("Backup Database Server",
        Description = "Backup the selected database to the provided file name.",
        NameTemplate = "Backup {DatabaseServerId} to {DestinationFilename}")]
    public class BackupDatabaseServer : ICommand
    {
        [DisplayName("Database Server")]
        public DatabaseServerId? DatabaseServerId { get; init; }
        
        [DisplayName("Destination Servers")]
        public DatabaseServerId[] DestinationServerIds { get; init; }
        
        [DisplayName("Destination Filename")]
        [Description("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis vel turpis vel diam dignissim euismod ut non velit. Integer eget.")]
        [StringPropertySuggestions(ResourceProviderName = "FilenameSuggestionProvider")]
        public string? DestinationFilename { get; init; }
        
        [DisplayName("Override backup")]
        [Description("Override any existing backup sets")]
        public bool OverrideBackup { get; init; }
        
        [DisplayName("Start At")]
        public DateTime? StartBackupDateTime { get; init; }
    }
}