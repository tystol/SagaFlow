using Rebus.Handlers;
using SagaFlow;
using SagaFlow.Status;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers
{
	public class BackupDatabaseServerHandler : IHandleMessages<BackupDatabaseServer>
    {
        private readonly ISagaFlowCommandContext _commandContext;

        public BackupDatabaseServerHandler(ISagaFlowCommandContext commandContext)
        {
            _commandContext = commandContext;
        }
        
        public async Task Handle(BackupDatabaseServer command)
        {
            Console.WriteLine($"Backing up database: {command.DatabaseServerId} to {command.DestinationFilename}.");

            for (var i = 0; i < 10; i ++)
            {
                if (i == 8 && command.OverrideBackup)
                    throw new Exception("Something went wrong");
                
                await _commandContext.UpdateProgress(i * 10);
                await Task.Delay(2000);
            }
        }
    }
}

