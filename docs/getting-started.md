# Getting Started

## Install

```bash
dotnet add package Mediation
```

## Register

```csharp
services.AddMediator(static configuration =>
{
    configuration
        .WithNoBehavior()
        .Register<Ping, Pong, PingHandler>();
});
```

## Send

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();
var pong = await mediator.Send(new Ping("hello"));
```

## Scan Assemblies

```csharp
services.AddMediator(static configuration =>
{
    configuration
        .WithNoBehavior()
        .RegisterForAssemblies(typeof(PingHandler).Assembly);
});
```

Use explicit registration when trim or Native AOT constraints matter.
