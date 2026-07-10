# Examples

## Request and Handler

```csharp
public sealed record CreateOrder(Guid CustomerId) : IRequest<OrderCreated>;
public sealed record OrderCreated(Guid OrderId);

public sealed class CreateOrderHandler : IRequestHandler<CreateOrder, OrderCreated>
{
    public ValueTask<OrderCreated> Handle(CreateOrder request, CancellationToken cancellationToken)
        => ValueTask.FromResult(new OrderCreated(Guid.NewGuid()));
}
```

## Behavior

```csharp
public readonly struct TimingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestPipelineContinuation<TResponse> nextHandler,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var response = await nextHandler().ConfigureAwait(false);
        var elapsed = Stopwatch.GetElapsedTime(started);
        Console.WriteLine($"{typeof(TRequest).Name}: {elapsed}");
        return response;
    }
}
```

## Registration with Behaviors

```csharp
services.AddMediator(static configuration =>
{
    configuration
        .WithBehaviors(typeof(TimingBehavior<,>))
        .Register<CreateOrder, OrderCreated, CreateOrderHandler>();
});
```
