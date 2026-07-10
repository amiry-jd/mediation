namespace Mediation.SampleConsole;

internal sealed record CreateUserResponse;

internal sealed record CreateUserRequest : IRequest<CreateUserResponse>;

internal sealed class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public ValueTask<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Message from request handler...");
        return ValueTask.FromResult(new CreateUserResponse());
    }
}

internal readonly struct LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestPipelineContinuation<TResponse> nextHandler,
        CancellationToken cancellationToken
        )
    {
        ArgumentNullException.ThrowIfNull(nextHandler);

        Console.WriteLine("Message from logging behavior...");
        Console.WriteLine(typeof(TRequest).Name);
        return await nextHandler().ConfigureAwait(false);
    }
}

internal readonly struct AuthBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestPipelineContinuation<TResponse> nextHandler,
        CancellationToken cancellationToken
        )
    {
        ArgumentNullException.ThrowIfNull(nextHandler);

        Console.WriteLine("Message from auth behavior...");
        Console.WriteLine(typeof(TRequest).Name);
        return await nextHandler().ConfigureAwait(false);
    }
}
