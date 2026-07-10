# Mediation

Mediation is a lightweight request/response mediator for .NET. It provides a small public API, Microsoft.Extensions.DependencyInjection integration, and precompiled struct-based pipeline behaviors for cross-cutting concerns.

## Features

- Request/response dispatch through `IMediator`.
- Scoped handler resolution through `IServiceProvider`.
- Ordered open-generic pipeline behaviors.
- Assembly scanning for `IRequestHandler<TRequest,TResponse>` implementations.
- Nullable-enabled, analyzer-clean public API.
- NuGet-ready package metadata, SourceLink, XML docs, symbols, API tracking, tests, benchmarks, and CI/CD automation.

## Installation

```bash
dotnet add package Mediation
```

## Quick Start

```csharp
using Mediation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMediator(static configuration =>
{
    configuration
        .WithNoBehavior()
        .Register<CreateUser, UserCreated, CreateUserHandler>();
});

using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();
var response = await mediator.Send(new CreateUser("Ada"));

public sealed record CreateUser(string Name) : IRequest<UserCreated>;
public sealed record UserCreated(string Name);

public sealed class CreateUserHandler : IRequestHandler<CreateUser, UserCreated>
{
    public ValueTask<UserCreated> Handle(CreateUser request, CancellationToken cancellationToken)
        => ValueTask.FromResult(new UserCreated(request.Name));
}
```

## Pipeline Behaviors

Behaviors must be open generic value types with a parameterless constructor.

```csharp
public readonly struct LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestPipelineContinuation<TResponse> nextHandler,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(typeof(TRequest).Name);
        return await nextHandler().ConfigureAwait(false);
    }
}

services.AddMediator(static configuration =>
{
    configuration
        .WithBehaviors(typeof(LoggingBehavior<,>))
        .Register<CreateUser, UserCreated, CreateUserHandler>();
});
```

## Configuration

- Use `WithNoBehavior()` for direct handler dispatch.
- Use `WithBehaviors(typeof(MyBehavior<,>))` to apply behaviors in registration order.
- Use `Register<TRequest,TResponse,THandler>()` for explicit registration.
- Use `RegisterForAssemblies(...)` to discover concrete handler classes.

## Examples

See `Mediation.SampleConsole` for a runnable ASP.NET Core sample and `tests/Mediation.Tests` for deterministic usage examples.

## FAQ

**Is this a replacement for MediatR?**  
It is a smaller mediator focused on request/response dispatch and struct pipeline behaviors.

**Are behaviors resolved from dependency injection?**  
No. Behaviors are currently parameterless structs. Use `serviceProvider` inside `Handle` to access scoped services when required.

**Is Native AOT supported?**  
The core package avoids runtime `DynamicInvoke`, but assembly scanning and expression compilation require dynamic code. AOT-sensitive applications should prefer explicit registration and validate publish settings.

## Troubleshooting

- `No request handler has been registered`: ensure the request type is registered explicitly or discovered by `RegisterForAssemblies`.
- Duplicate registration errors: each concrete request type can have only one pipeline registration.
- Behavior validation errors: pass open generic struct types such as `typeof(LoggingBehavior<,>)`.

## Contributing

See `CONTRIBUTING.md` for development, validation, and API review requirements.

## License

Mediation is licensed under the MIT License. See `LICENSE`.
