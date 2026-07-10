namespace Mediation;

/// <summary>
/// Marks a type as a request that produces a response.
/// </summary>
/// <typeparam name="TResponse">The response type returned by the request handler.</typeparam>
public interface IRequest<TResponse>
{
}

/// <summary>
/// Handles a request and returns its response.
/// </summary>
/// <typeparam name="TRequest">The request type handled by this handler.</typeparam>
/// <typeparam name="TResponse">The response type returned by this handler.</typeparam>
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="request">The request instance to handle.</param>
    /// <param name="cancellationToken">A token used to cancel request processing.</param>
    /// <returns>The response produced by the handler.</returns>
    ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken);
}
