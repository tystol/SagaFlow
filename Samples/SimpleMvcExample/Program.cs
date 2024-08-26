using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Sagas;
using Rebus.Subscriptions;
using Rebus.Timeouts;
using Rebus.Transport;
using Rebus.Transport.InMem;
using SagaFlow.SignalR;
using SimpleMvcExample.CommandHandlers;
using SimpleMvcExample.Messages;
using SimpleMvcExample.ResourceProviders;
using SimpleMvcExample.StrongTyping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<SimpleTaskHandler>();
builder.Services.AddScoped<RequestHandler>();
builder.Services.AddScoped<PeriodicJobHandler>();
builder.Services.AddScoped<PeriodicAlternativeJobHandler>();
builder.Services.AddScoped<SendMessageToTenant>();
builder.Services.AddScoped<BackupDatabaseServerHandler>();
builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
    });

const string inputQueueName = "sagaflow.commands";
const string backgroundJobsQueueName = "sagaflow.jobs";

var dbProvider = builder.Configuration.GetValue<string>("Database:Provider");
var db = builder.Configuration.GetValue<string>("Database:ConnectionString");

void ConfigureRebusTransport(StandardConfigurer<ITransport> t)
{
    if (dbProvider == "sqlserver")
        t.UseSqlServer( new SqlServerTransportOptions(db), inputQueueName);
    else if (dbProvider == "postgres")
        t.UsePostgreSql(db, "messages", inputQueueName);
    else
        t.UseInMemoryTransport(new InMemNetwork(),inputQueueName);
}
void ConfigureRebusTimeoutStorage(StandardConfigurer<ITimeoutManager> s)
{
    if (dbProvider == "sqlserver")
        s.StoreInSqlServer(db, "Timeouts");
    else if (dbProvider == "postgres")
        s.StoreInPostgres(db, "timeouts");
    else
        s.StoreInMemory();
}
void ConfigureRebusSubscriptions(StandardConfigurer<ISubscriptionStorage> s)
{
    if (dbProvider == "sqlserver")
        s.StoreInSqlServer(db, "Subscriptions", isCentralized: true);
    else if (dbProvider == "postgres")
        s.StoreInPostgres(db, "subscriptions", isCentralized: true);
    else
    {
        // Not needed as of Rebus v8, as the in-memory transport also registers itself as subscription storage.
        // s.StoreInMemory();
    }
}
void ConfigureRebusSagaStorage(StandardConfigurer<ISagaStorage> s)
{
    if (dbProvider == "sqlserver")
        s.StoreInSqlServer(db, "Sagas", "SagaIndex");
    else if (dbProvider == "postgres")
        s.StoreInPostgres(db, "saga-data", "saga-index");
    else
        s.StoreInMemory();
}

// TODO: Look at running the web app as only for producing Saga msgs,
// and a separate worker app for actually running the saga workflows.
//services.AddSagaFlow(o => o.WithTransport(t => t.UsePostgreSqlAsOneWayClient(db, "messages")));
builder.Services.AddSagaFlow(o => o
    .AddResourceProvidersFromAssemblyOf<SampleTenantProvider>()
    .AddHandlersFromAssemblyOf<SimpleTaskHandler>()
    .AddCommandsOfType<ICommand>()
    .WithOptions(o => o.RetryStrategy(maxDeliveryAttempts: 3))
    //.AddCommandFromEvent<StartSimpleTaskRequested>()
    .WithLogging(l => l.Console())
    .WithTransport(ConfigureRebusTransport)
    .WithTimeoutStorage(ConfigureRebusTimeoutStorage)
    .WithSubscriptionStorage(ConfigureRebusSubscriptions)
    .WithSagaStorage(ConfigureRebusSagaStorage)
    .WithRouting(r => r.TypeBased()
        .MapAssemblyDerivedFrom<ICommand>(inputQueueName)
        .Map<Request>(inputQueueName)//backgroundJobsQueueName)
        .Map<Reply>(inputQueueName))//backgroundJobsQueueName))
    .WithSignalR()
    .WithSystemUsername("System")
    .WithAnonymousUsername("Anonymous")
    //.WithTimeoutStorage(t => t.StoreInPostgres(db, "timeouts"))
    //, apiBasePath:"ocp"
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSagaFlow();

app.Run();

Console.WriteLine("Server shutdown complete.");

public class TestSagaData : SagaData
{
    public string CaseNumber { get; set; }
    public int MessagesReceived { get; set; }
}

public class TestSaga : Saga<TestSagaData>, IAmInitiatedBy<StartSimpleTask>, IHandleMessages<Reply>
{
    private readonly IBus bus;

    public TestSaga(IBus bus)
    {
        // TODO: Need to inject a separate worker/saga bus for handling and sending saga messages, outside
        // the scope of the sagaflow bus that should only be used to receive API messages and send them
        // as sagaflow messages.
        // OR - only do sagaflow metadata addition when sending messages from API controller.
        this.bus = bus;
    }
    protected override void CorrelateMessages(ICorrelationConfig<TestSagaData> config)
    {
        config.Correlate<StartSimpleTask>(m => m.Message, d => d.CaseNumber);
        config.Correlate<Reply>(m => m.CaseNumber, d => d.CaseNumber);
    }

    public async Task Handle(StartSimpleTask message)
    {
        Data.MessagesReceived++;
        
        Console.WriteLine($"{Data.MessagesReceived} messages received to case {Data.CaseNumber}.");
        await bus.Send(new Request{CaseNumber = Data.CaseNumber});
        
        if (!CompleteIfNeeded())
            await Task.Delay(500);
    }

    public async Task Handle(Reply message)
    {
        Data.MessagesReceived++;
        Console.WriteLine($"{Data.MessagesReceived} messages received to case {Data.CaseNumber}.");
        
        if (!CompleteIfNeeded())
            await bus.Defer(TimeSpan.FromMilliseconds(500), new Request {CaseNumber = Data.CaseNumber});
    }

    private bool CompleteIfNeeded()
    {
        if (Data.MessagesReceived >= 10)
        {
            MarkAsComplete();
            return true;
        }

        return false;
    }
}

public record Request
{
    public string CaseNumber { get; set; }
}

public record Reply
{
    public string CaseNumber { get; set; }
}