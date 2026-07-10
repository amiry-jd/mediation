using Microsoft.Extensions.DependencyInjection;

namespace Mediation;

/// <summary>
/// Defines a dispatcher for sending requests to their registered handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the matching registered request handler.
    /// </summary>
    /// <typeparam name="TResponse">The response type produced by the request.</typeparam>
    /// <param name="request">The request instance to dispatch.</param>
    /// <param name="cancellationToken">A token used to cancel request processing.</param>
    /// <returns>The response returned by the matching request handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler pipeline has been registered for the request type.</exception>
    ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Dispatches request objects to handlers registered in an <see cref="IServiceProvider" />.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly MediatorRegistry registry;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve request handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is <see langword="null" />.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.serviceProvider = serviceProvider;
        registry = serviceProvider.GetRequiredService<MediatorRegistry>();
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return registry.Send(serviceProvider, request, cancellationToken);
    }
}
