using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using NCrontab.Scheduler;

namespace SagaFlow.Recurring;

internal class CronRecurringCommandsBackgroundService : BackgroundService, IRecurringCommandScopeAccessor
{
    private readonly SagaFlowModule sagaFlowModule;
    private readonly IServiceProvider rootScope;
    private readonly AsyncLocal<IServiceProvider?> currentCommandScope = new();

    public CronRecurringCommandsBackgroundService(
        SagaFlowModule sagaFlowModule,
        IServiceProvider rootScope)
    {
        this.sagaFlowModule = sagaFlowModule;
        this.rootScope = rootScope;
    }

    IServiceProvider? IRecurringCommandScopeAccessor.Scope => currentCommandScope.Value;
    
    // TODO:
    //  - Persistant storage mechanism
    //  - UI to show currently registered recurring commands, and ability to trigger ad-hoc
    //  - Utilize storage to guarantee scheduling (eg. handle if down during trigger time, handle
    //    multiple instances triggering same job twice for same time, etc)
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var schedulerScope = rootScope.CreateScope();
        using var scheduler = new Scheduler(schedulerScope.ServiceProvider.GetService<ILogger<Scheduler>>());
        var bus = schedulerScope.ServiceProvider.GetRequiredService<ISagaFlowCommandBus>();

        foreach (var command in sagaFlowModule.Commands.Where(c => c.IsRecurringCommand))
        {
            scheduler.AddTask( CrontabSchedule.Parse(command.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true}), async ct =>
            {
                var commandMessage = Activator.CreateInstance(command.CommandType) ?? throw new ArgumentException($"Could not create instance of type {command.CommandType}");
                currentCommandScope.Value = schedulerScope.ServiceProvider;
                await bus.Send(commandMessage);
                currentCommandScope.Value = null;
            });
        }
        await scheduler.StartAsync(stoppingToken);
    }
}