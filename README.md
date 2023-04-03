# SagaFlow
A framework to quickly scaffold an application based on a declarative message DTO format.
Define your messages (commands and/or events) as POCO DTOs, and have a fully functional UI and underlying queue/workflow engine automatically built to handle the messages.
You write message classes, plus your business logic within handlers and/or sagas - everything else is generated at runtime.

## Message-based Design
The design has been inspired by [ServiceStack](https://servicestack.net/).
Rather than repeat the benefits of a message based design here, please read up on the great docs over at ServiceStack. eg:
- [Design message-based APIs](https://docs.servicestack.net/design-message-based-apis)
- [What is a message-based web service](https://docs.servicestack.net/what-is-a-message-based-web-service)
- [Advantages of message-based web services](https://docs.servicestack.net/advantages-of-message-based-web-services)

## Getting Started
SagaFlow heavily relies on [Rebus](https://github.com/rebus-org/Rebus) under the covers to facilate the messaging and saga/workflow engine. It is worth familiarizing yourself with Rebus and service buses in general.

### Define your messages
For example, a command to add a new tenant to your infrastructure:
```C#
public record AddTenantCommand
{
	public string? Name { get; init; }
}
```
### Implement your logic
Application logic is written inside of message handlers. These are facilitated directly via [Rebus - read up more here](https://github.com/rebus-org/Rebus/wiki/Getting-started).

```C#
public class AddTenantCommandHandler : IHandleMessages<AddTenantCommand>
{
    public Task Handle(AddTenantCommand command)
    {
        var db = ...;
        return db.Tenants.Add(new Tenant { Name = command.Name });
    }
}
```

### Resource lists
As well as executing commands, an application almost always needs to display back some lists of resources (often with further commands possible per resource).
To achieve this within SagaFlow, implement a resource provider:
```C#
public class TenantProvider : IResourceListProvider<Tenant>
{
    public Task<IList<Tenant>> GetAll()
    {
        var db = ...;
        return db.Tenants;
    }
}

public class Tenant
{
    public Guid Id { get; init; }
    public string Name { get; init; }
}
```

### Register middleware
SagaFlow relies on installing its own middleware to publish your app schema, serve the UI, serve your app's resources and receive messages.
Firstly configure your providers, handlers and the core SagaFlow service (and underlying Rebus service bus):
```C#
builder.Services.AddScoped<IResourceListProvider<Tenant>>(s => new TenantProvider());
builder.Services.AddScoped<AddTenantCommandHandler>();
builder.Services.AddSagaFlow(o => o
    .AddResourceProvidersFromAssemblyOf<TenantProvider>()
    .AddCommandsOfType<AddTenantCommand>()
    .WithLogging(l => l.Console())
    .WithTransport(t => t.UsePostgreSql(db, "messages", "ops-panel.workflow"))
    .WithSubscriptionStorage(s => s.StoreInPostgres(db, "subscriptions", isCentralized: true))
    .WithSagaStorage(s => s.StoreInPostgres(db, "saga-data", "saga-index"))
    .WithRouting(r => r.TypeBased().MapAssemblyOf<AddTenantCommand>("ops-panel"))
    );
```

### Run your app
Your app will serve the generated SagaFlow UI under **todo...**
**Insert screenshot of above UI here**