using FluentTestScaffold.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SimpleMvcExample;
using SimpleMvcExample.Migrations;

namespace Sagaflow.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        /// here you will use the builder to make changes to the web host before you start testing
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            // override configurations
        });
        
        builder.ConfigureTestServices(services =>
        {
            services.ReplaceDbContextWithInMemoryProvider<ApplicationDbContext>();
            // override service registrations, eg. replace registration of interfaces to concrete types with mocks.
            // services.AddSingleton<ApplicationDbContext>(_ => InMemoryDbContextFactory.Create());
        });
    }
}