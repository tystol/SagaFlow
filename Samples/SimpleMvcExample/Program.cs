using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Sagas;
using Rebus.Subscriptions;
using Rebus.Transport;
using SagaFlow;
using SimpleMvcExample.CommandHandlers;
using SimpleMvcExample.Messages;
using SimpleMvcExample.ResourceProviders;
using SimpleMvcExample.StrongTyping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// TODO: Find better way of handling the 2 generic overloads of resource providers. Should be able to register via 2 param type.
//builder.Services.AddScoped<IResourceListProvider<SampleTenant, Guid>>(s => new SampleTenantProvider());
//builder.Services.AddScoped<IResourceListProvider<DatabaseServer, DatabaseServerId>>(s => new SampleDatabaseServerProvider());
builder.Services.AddScoped<IResourceListProvider<SampleTenant>>(s => new SampleTenantProvider());
builder.Services.AddScoped<IResourceListProvider<DatabaseServer>>(s => new SampleDatabaseServerProvider());
builder.Services.AddScoped<SimpleTaskHandler>();
builder.Services.AddScoped<SendMessageToTenant>();
builder.Services.AddScoped<BackupDatabaseServerHandler>();
builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
    });

var dbProvider = builder.Configuration.GetValue<string>("Database:Provider");
var db = builder.Configuration.GetValue<string>("Database:ConnectionString");

void ConfigureRebusTransport(StandardConfigurer<ITransport> t)
{
    if (dbProvider == "sqlserver")
        t.UseSqlServer(db, "test.workflow");
    else
        t.UsePostgreSql(db, "messages", "test.workflow");
}
void ConfigureRebusSubscriptions(StandardConfigurer<ISubscriptionStorage> s)
{
    if (dbProvider == "sqlserver")
        s.StoreInSqlServer(db, "Subscriptions", isCentralized: true);
    else
        s.StoreInPostgres(db, "subscriptions", isCentralized: true);
}
void ConfigureRebusSagaStorage(StandardConfigurer<ISagaStorage> s)
{
    if (dbProvider == "sqlserver")
        s.StoreInSqlServer(db, "Sagas", "SagaIndex");
    else
        s.StoreInPostgres(db, "saga-data", "saga-index");
}

// TODO: Look at running the web app as only for producing Saga msgs,
// and a separate worker app for actually running the saga workflows.
//services.AddSagaFlow(o => o.WithTransport(t => t.UsePostgreSqlAsOneWayClient(db, "messages")));
builder.Services.AddSagaFlow(o => o
    .AddResourceProvidersFromAssemblyOf<SampleTenantProvider>()
    .AddHandlersFromAssemblyOf<SimpleTaskHandler>()
    .AddCommandsOfType<ICommand>()
    //.AddCommandFromEvent<StartSimpleTaskRequested>()
    .WithLogging(l => l.Console())
    .WithTransport(ConfigureRebusTransport)
    .WithSubscriptionStorage(ConfigureRebusSubscriptions)
    .WithSagaStorage(ConfigureRebusSagaStorage)
    .WithRouting(r => r.TypeBased().MapAssemblyOf<ICommand>("ops-panel"))
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
else
{
    app.UseCors(p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
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

