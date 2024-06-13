using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SimpleMvcExample.Migrations;

namespace Sagaflow.Tests;

public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var dbContext = new ApplicationDbContext(contextOptions);

        return dbContext;
    }
}