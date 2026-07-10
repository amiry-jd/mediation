namespace Mediation;

internal sealed class MediatorRegistry
{
    private readonly Dictionary<Type, PipelineRegistration> registrations = [];

    public void Add<TRequest, TResponse>(
        CompiledPipeline<TRequest, TResponse> pipeline)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var requestType = typeof(TRequest);
        if (!registrations.TryAdd(requestType, new PipelineRegistration<TRequest, TResponse>(pipeline)))
        {
            throw new InvalidOperationException(
                $"A request handler has already been registered for request type '{requestType.FullName}'.");
        }
    }

    public ValueTask<TResponse> Send<TResponse>(
        IServiceProvider serviceProvider,
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        if (!registrations.TryGetValue(requestType, out var registration))
        {
            throw new InvalidOperationException(
                $"No request handler has been registered for request type '{requestType.FullName}'.");
        }

        return registration.Invoke(serviceProvider, request, cancellationToken);
    }
}
