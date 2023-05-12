using Rebus.Handlers;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers
{
	public class BackupDatabaseServerHandler : IHandleMessages<BackupDatabaseServer>
    {
        public Task Handle(BackupDatabaseServer command)
        {
            Console.WriteLine($"Backing up database: {command.DatabaseServerId} to {command.DestinationFilename}.");
            return Task.Delay(3000);
        }
    }
}

