namespace Mediation.SampleConsole;

public record CreateUserResponse { }

public record CreateUserRequest : IRequest<CreateUserResponse> { }

public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public ValueTask<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken ct)
    {
        Console.WriteLine("Message from request handler...");
        return ValueTask.FromResult(new CreateUserResponse());
    }
}

public readonly struct LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
        )
    {
        Console.WriteLine("Message from logging behavior...");
        Console.WriteLine(typeof(TRequest).Name);
        return await next();
    }
}

public readonly struct AuthBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
        )
    {
        Console.WriteLine("Message from auth behavior...");
        Console.WriteLine(typeof(TRequest).Name);
        return await next();
    }
}