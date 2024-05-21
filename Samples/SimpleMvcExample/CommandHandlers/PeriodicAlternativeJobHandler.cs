using Rebus.Handlers;
using SimpleMvcExample.Messages;

namespace SimpleMvcExample.CommandHandlers;

public class PeriodicAlternativeJobHandler : IHandleMessages<StartPeriodicAlternativeJob>
{
    public async Task Handle(StartPeriodicAlternativeJob message)
    {
        Console.WriteLine($"Periodic alternative Job Started...");
        await Task.Delay(7000);
        Console.WriteLine($"Periodic alternative Job Finished.");
    }
}