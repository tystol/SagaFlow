using Rebus.Config;
using Rebus.Routing.TypeBased;
using SagaFlow;
using SimpleMvcExample.CommandHandlers;
using SimpleMvcExample.Messages;
using SimpleMvcExample.ResourceProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IResourceListProvider<Tenant>>(s => new SampleTenantProvider());
builder.Services.AddScoped<SimpleTaskHandler>();
builder.Services.AddControllersWithViews();

var db = builder.Configuration.GetValue<string>("Database:ConnectionString");
// TODO: Look at running the web app as only for producing Saga msgs,
// and a separate worker app for actually running the saga workflows.
//services.AddSagaFlow(o => o.WithTransport(t => t.UsePostgreSqlAsOneWayClient(db, "messages")));
builder.Services.AddSagaFlow(o => o
    .AddResourceProvidersFromAssemblyOf<SampleTenantProvider>()
    .AddHandlersFromAssemblyOf<SimpleTaskHandler>()
    .AddCommandsOfType<ICommand>()
    //.AddCommandFromEvent<StartSimpleTaskRequested>()
    .WithLogging(l => l.Console())
    .WithTransport(t => t.UsePostgreSql(db, "messages", "test.workflow"))
    .WithSubscriptionStorage(s => s.StoreInPostgres(db, "subscriptions", isCentralized: true))
    .WithSagaStorage(s => s.StoreInPostgres(db, "saga-data", "saga-index"))
    .WithRouting(r => r.TypeBased().MapAssemblyOf<ICommand>("ops-panel"))
    //.WithTimeoutStorage(t => t.StoreInPostgres(db, "timeouts"))
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

