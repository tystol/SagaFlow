using System.ComponentModel;

namespace SimpleMvcExample.Messages
{
    [DisplayName("Periodic Job [cron: 0 */5 * * * *]")] // Every 15 seconds
    public record StartPeriodicJob : ICommand
    {
        
    }
}