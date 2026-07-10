using Microsoft.Extensions.DependencyInjection;

namespace Mediation;

internal abstract class PipelineRegistration
{
    public abstract ValueTask<TResponse> Invoke<TResponse>(
        IServiceProvider serviceProvider,
        IRequest<TResponse> request,
        CancellationToken cancellationToken);
}

internal sealed class PipelineRegistration<TRequest, TResponse>(
    CompiledPipeline<TRequest, TResponse> pipeline) : PipelineRegistration
    where TRequest : IRequest<TResponse>
{
    public override ValueTask<TValue> Invoke<TValue>(
        IServiceProvider serviceProvider,
        IRequest<TValue> request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(request);

        if (request is not TRequest typedRequest)
        {
            throw new InvalidOperationException(
                $"Registered request type '{typeof(TRequest).FullName}' cannot handle request type '{request.GetType().FullName}'.");
        }

        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var response = pipeline(serviceProvider, handler, typedRequest, cancellationToken);

        if (typeof(TValue) == typeof(TResponse))
        {
            return CastResponse<TValue>(response);
        }

        throw new InvalidOperationException(
            $"Registered response type '{typeof(TResponse).FullName}' cannot be returned as '{typeof(TValue).FullName}'.");
    }

    private static async ValueTask<TValue> CastResponse<TValue>(ValueTask<TResponse> response)
    {
        var result = await response.ConfigureAwait(false);
        return (TValue)(object?)result!;
    }
}
