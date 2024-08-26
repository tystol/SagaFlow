using Rebus.Bus;
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
    
    
    public class RequestHandler : IHandleMessages<Request>
    {
        private readonly IBus bus;

        public RequestHandler(IBus bus)
        {
            this.bus = bus;
        }
        public Task Handle(Request message)
        {
            Console.WriteLine($"Request Started: {message.CaseNumber}");
            return bus.Reply(new Reply{CaseNumber = message.CaseNumber});
        }
    }
}

