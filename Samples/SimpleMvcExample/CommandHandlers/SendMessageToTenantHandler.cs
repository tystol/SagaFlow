using Rebus.Handlers;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers
{
	public class SendMessageToTenantHandler : IHandleMessages<SendMessageToTenant>
    {
        public async Task Handle(SendMessageToTenant command)
        {
            Console.WriteLine($"Sending: {command.Message} to tenant {command.TenantId}...");
            await Task.Delay(1000);
            Console.WriteLine("Done");
        }
    }
}

