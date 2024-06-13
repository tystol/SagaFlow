using SagaFlow.Interfaces;

namespace SimpleMvcExample.Messages
{
    // Alternative way to specify a periodic job on a cron expression using SagaFlow.Interfaces
    [Command("Start Periodic Job", Cron = "0 */5 * * * *")] // every 15 seconds
    // [Command("Start Periodic Job", Cron = "*/15 * * * * *")] // every 15 seconds
    public class StartPeriodicAlternativeJob : ICommand
    {
        
    }
}