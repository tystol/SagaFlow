using Rebus.Handlers;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers
{
	public class SimpleTaskHandler : IHandleMessages<StartSimpleTask>
    {
        public Task Handle(StartSimpleTask command)
        {
            Console.WriteLine($"Simple Task Started: {command.Message}");
            return Task.Delay(3000);
        }
    }
}

