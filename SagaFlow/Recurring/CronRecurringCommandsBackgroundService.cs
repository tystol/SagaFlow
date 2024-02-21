using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using NCrontab.Scheduler;
using Rebus.Bus;

namespace SagaFlow.Recurring;

internal class CronRecurringCommandsBackgroundService : BackgroundService
{
    private readonly SagaFlowModule sagaFlowModule;
    private readonly IServiceProvider services;

    public CronRecurringCommandsBackgroundService(
        SagaFlowModule sagaFlowModule,
        IServiceProvider services)
    {
        this.sagaFlowModule = sagaFlowModule;
        this.services = services;
    }
    
    // TODO:
    //  - Persistant storage mechanism
    //  - UI to show currently registered recurring commands, and ability to trigger ad-hoc
    //  - Utilize storage to guarantee scheduling (eg. handle if down during trigger time, handle
    //    multiple instances triggering same job twice for same time, etc)
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = new Scheduler(services.GetService<ILogger<Scheduler>>());
        var bus = services.GetRequiredService<IBus>();

        foreach (var command in sagaFlowModule.Commands.Where(c => c.IsRecurringCommand))
        {
            scheduler.AddTask( CrontabSchedule.Parse(command.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true}), ct =>
            {
                var commandMessage = Activator.CreateInstance(command.CommandType);
                return bus.Send(commandMessage);
            });
        }
        await scheduler.StartAsync(stoppingToken);
    }
}