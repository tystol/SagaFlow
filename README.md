# SagaFlow
An opinionated framework to quickly scaffold an application based on a declarative message DTO format.
Define your messages (commands and/or events) as POCO DTOs, and have a fully functional UI and underlying
queue/workflow engine automatically built to handle the messages.
You write message contracts and your business logic within handlers and/or sagas - everything else is generated
at runtime.

## Message-based Design
The design has been inspired by [ServiceStack](https://servicestack.net/).
Rather than repeat the benefits of a message based design here, please read up on the great docs over at ServiceStack. eg:
- [Design message-based APIs](https://docs.servicestack.net/design-message-based-apis)
- [What is a message-based web service](https://docs.servicestack.net/what-is-a-message-based-web-service)
- [Advantages of message-based web services](https://docs.servicestack.net/advantages-of-message-based-web-services)

## Getting Started
SagaFlow heavily relies on [Rebus](https://github.com/rebus-org/Rebus) under the covers to facilitate the messaging
and saga/workflow engine. It is worth familiarizing yourself with Rebus and service buses in general before
getting started if not already familiar. Specifically:
- [Rebus Introduction](https://github.com/rebus-org/Rebus/wiki/Introduction)
- [Handing off work](https://github.com/rebus-org/Rebus/wiki/Handing-off-work)
- [Coordinating stuff that happens over time](https://github.com/rebus-org/Rebus/wiki/Coordinating-stuff-that-happens-over-time)

To use the SagaFlow framework:

### Define your message contracts
For example, a command to add a new tenant to your infrastructure:
```C#
public record AddTenantCommand
{
	public string? Name { get; init; }
}
```

#### Scheduled re-occuring commands
For commands that are executed on a scheduled time. 

Using the CommandAttribute, add [SagaFlow.Interfaces](https://www.nuget.org/packages/SagaFlow.Interfaces) package to the class library containing your messages. Decorate the message with the CommandAttribute with a cron expression for when the job should run.
```C#
[Command("Take a shower", Cron = "30 17 * * * *")] // Takes a shower 5:30 pm everyday 
public record TakeAShower { }
```

If you don't want to use the [SagaFlow.Interfaces](https://www.nuget.org/packages/SagaFlow.Interfaces) package, then you can specify the cron expression as part of the DisplayNameAttribute. This allows you to keep your messages library decoupled from SagaFlow.
```C#
[DisplayName("Take a shower [cron: 30 17 * * * *]")] // Takes a shower 5:30 pm everyday 
public record TakeAShower { }
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

### Web Components

Sagaflow can expose a set of web components which can be used in your application.

To import the Sagaflow webcomponents to your front end, you will need to import the SagaFlow javascript module:

```html
<script src="/sagaflow/web-components.js" type="module"></script>
```

In-case you have setup SagaFlow to use a different route for its webapi calls then you will need to import the SagaFlow module as such:

```html

<script type="module">
    import "/[custom-sagaflow-route]/web-components.js";
    
    window.SagaFlow.initialize("[custom-sagaflow-route]")
</script>
```

or if you don't want to use the attached SagaFlow from the window object.

```html

<script type="module">
    import sagaFlow from "/[custom-sagaflow-route]/web-components.js";

    sagaFlow.initialize("[custom-sagaflow-route]")
</script>

#### sf-command-form - Command form

A Sagaflow web-component to display a simple form that is used to submit a command message to SagaFlow to run.

For example, if you have a message that looks like this:

```csharp
public class BackupDatabaseServer : ICommand
{
    [DisplayName("Database Server")]
    public DatabaseServerId? DatabaseServerId { get; init; }
    
    [DisplayName("Destination Filename")]
    public string? DestinationFilename { get; init; }
}
```

With a set of Database Server resources that looks like this:

```csharp
public class SampleDatabaseServerProvider : IResourceListProvider<DatabaseServer, DatabaseServerId>
{
    private static List<DatabaseServer> databases = Enumerable.Range(0, 10)
        .Select(t => new DatabaseServer
        {
            Id = new DatabaseServerId(String.Format("server-{0:00}",t+1)),
            Name = "Server " + (t + 1),
        })
        .ToList();

    public Task<IList<DatabaseServer>> GetAll()
    {
        return Task.FromResult((IList<DatabaseServer>)databases);
    }
}

public class DatabaseServer : IResource<DatabaseServerId>
{
    public DatabaseServerId Id { get; init; }
    public string Name { get; init; }
}
```

Then to display the Sagaflow command form to allow the user select a Database Server and to enter a Destination Filename, in your html markup:

```html

<sf-command-form commandId="backup-database-server"></sf-command-form>

```

The form will display a Drop Down list for the user to select a database server resource, and a input box to enter a destination filename.

You can listen for the following SagaFlow DOM events for when a SagaFlow command has been successful sent to the server to be processed:
- sf-command-success - The SagaFlow command was successful submitted to the server to be processed;
- sf-command-error - An error was encountered submitting the command to the server to be processed;
- sf-command-complete - The process of submitting the command to the server has completed, regardless if it was successful or not.

The following will present a SagaFlow command form, when the user has completed the form and click on the run button it will submit the command to the SagaFlow server to run.  The front end will listen for the SagaFlow DOM events to present a message to the user indicating success.
```html 
<script>
    document.addEventListener('sf-command-success', (e) => alert("Successful"));
</script>

<sf-command-form commandId="backup-database-server"></sf-command-form>

```