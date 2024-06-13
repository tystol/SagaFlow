using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Sagas;
using Rebus.Subscriptions;
using Rebus.Transport;
using Rebus.Transport.InMem;
using SagaFlow.SignalR;
using SimpleMvcExample.Areas.Identity.Data;
using SimpleMvcExample.CommandHandlers;
using SimpleMvcExample.Controllers.Api;
using SimpleMvcExample.Messages;
using SimpleMvcExample.Migrations;
using SimpleMvcExample.ResourceProviders;
using SimpleMvcExample.StrongTyping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Setup for Asp.net Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<SimpleTaskHandler>();
builder.Services.AddScoped<PeriodicJobHandler>();
builder.Services.AddScoped<PeriodicAlternativeJobHandler>();
builder.Services.AddScoped<SendMessageToTenant>();
builder.Services.AddScoped<BackupDatabaseServerHandler>();

//Register concrete implementations of provider so they can be used in the controllers. Not required for SagaFlow
builder.Services.AddScoped<JobsResourceProvider>();

builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
    // .AddJsonOptions(options =>
    // {
    //     options.JsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
    // });

builder.Services.AddControllers(c =>
{
    c.RespectBrowserAcceptHeader = true;
    c.Conventions.Add(new ApiAuthorizationConvention());
}).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
    });

const string inputQueueName = "sagaflow.sampleapp";

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
    .WithOptions(o => o.RetryStrategy(maxDeliveryAttempts: 1)) // Set the number of allow retries, here we set to only try one attempt we will report the error back to the user via SignalR.
    //.AddCommandFromEvent<StartSimpleTaskRequested>()
    .WithTransport(ConfigureRebusTransport)
    .WithSubscriptionStorage(ConfigureRebusSubscriptions)
    .WithSagaStorage(ConfigureRebusSagaStorage)
    .WithRouting(r => r.TypeBased().MapAssemblyOf<ICommand>(inputQueueName))
    .WithSignalR()
    .WithSystemUsername("System")
    .WithAnonymousUsername("Anonymous")
    //.WithTimeoutStorage(t => t.StoreInPostgres(db, "timeouts"))
    //, apiBasePath:"ocp"
    );

// Configure default authentication
builder.Services.AddAuthentication();

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    // Define policy for authenticated users
    options.AddPolicy("RequireAuthenticatedUser", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// builder.Services.AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//     })
//     .AddCookie(options => {
//     // options.Cookie.SameSite = SameSiteMode.Lax;
//     // options.LoginPath = "/Account/Login";
//     // options.AccessDeniedPath = "/Account/AccessDenied";
//
//     options.Events = new CookieAuthenticationEvents
//     {
//         OnRedirectToLogin = ctx =>
//         {
//             if (IsApiRequest(ctx.Request))
//             {
//                 ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                 ctx.Response.ContentType = "application/json";
//                 var result = JsonSerializer.Serialize(new { error = "Unauthorized" });
//                 return ctx.Response.WriteAsync(result);
//             }
//             else
//             {
//                 ctx.Response.Redirect(ctx.RedirectUri);
//                 return Task.CompletedTask;
//             }
//         },
//         OnRedirectToAccessDenied = ctx =>
//         {
//             if (IsApiRequest(ctx.Request))
//             {
//                 ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
//                 ctx.Response.ContentType = "application/json";
//                 var result = JsonSerializer.Serialize(new { error = "Forbidden" });
//                 return ctx.Response.WriteAsync(result);
//             }
//             else
//             {
//                 ctx.Response.Redirect(ctx.RedirectUri);
//                 return Task.CompletedTask;
//             }
//         }
//     };
// });

// builder.Services.AddAuthorization();

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
    app.UseMigrationsEndPoint();
    app.UseCors(p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use custom middleware to handle anti-forgery token for API requests
app.UseMiddleware<IgnoreAntiforgeryMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthchecks");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// For Asp.net Identity UI
app.MapRazorPages();

// Apply authorization to API endpoints
app.Use(async (context, next) =>
{
    // Check if it's an API request
    if (IsApiRequest(context.Request))
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<ApiControllerAttribute>() != null ||
            context.Request.Path.StartsWithSegments("/api"))
        {
            // Apply authorization policy to API controllers
            var policy = context.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>()
                .GetPolicyAsync("RequireAuthenticatedUser");
            var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
            var authResult = await authorizationService.AuthorizeAsync(context.User,"RequireAuthenticatedUser");
            if (!authResult.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
    }
    await next();
});


app.UseSagaFlow();

app.Run();

bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api") || request.Headers["Accept"].ToString().Contains("application/json");
}

namespace SimpleMvcExample
{
    public partial class Program { }
}

