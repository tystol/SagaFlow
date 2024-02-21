using Rebus.Handlers;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers
{
	public class PeriodicJobHandler : IHandleMessages<StartPeriodicJob>
    {
        public async Task Handle(StartPeriodicJob command)
        {
            Console.WriteLine($"Periodic Job Started...");
            await Task.Delay(7000);
            Console.WriteLine($"Periodic Job Finished.");
        }
    }
}

