using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Mediation;

/// <summary>
/// Represents the next handler or behavior in a request pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type produced by the request pipeline.</typeparam>
/// <returns>The response returned by the next pipeline component.</returns>
public delegate ValueTask<TResponse> RequestPipelineContinuation<TResponse>();

/// <summary>
/// Defines a behavior that can wrap request handling with cross-cutting logic.
/// </summary>
/// <typeparam name="TRequest">The request type handled by the behavior.</typeparam>
/// <typeparam name="TResponse">The response type produced by the request.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Handles a request before or after invoking the next pipeline component.
    /// </summary>
    /// <param name="serviceProvider">The current service provider.</param>
    /// <param name="request">The request being processed.</param>
    /// <param name="nextHandler">The next component in the pipeline.</param>
    /// <param name="cancellationToken">A token used to cancel request processing.</param>
    /// <returns>The response returned by the request pipeline.</returns>
    ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestPipelineContinuation<TResponse> nextHandler,
        CancellationToken cancellationToken);
}

internal delegate ValueTask<TResponse>
    CompiledPipeline<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        IRequestHandler<TRequest, TResponse> handler,
        TRequest request,
        CancellationToken cancellationToken)
    where TRequest : IRequest<TResponse>;

internal static class PipelineCompiler
{
    [RequiresUnreferencedCode("Behavior type construction uses reflection and is not compatible with trimming.")]
    [RequiresDynamicCode("Behavior type construction uses MakeGenericType and expression compilation.")]
    public static CompiledPipeline<TRequest, TResponse> Compile<TRequest, TResponse>(
        IReadOnlyList<Type> behaviorTypes)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(behaviorTypes);

        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var handlerParameter = Expression.Parameter(typeof(IRequestHandler<TRequest, TResponse>), "handler");
        var requestParameter = Expression.Parameter(typeof(TRequest), "request");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        Expression current = Expression.Call(
            handlerParameter,
            nameof(IRequestHandler<TRequest, TResponse>.Handle),
            typeArguments: null,
            requestParameter,
            cancellationTokenParameter);

        for (int index = behaviorTypes.Count - 1; index >= 0; index--)
        {
            var closedBehaviorType = behaviorTypes[index].MakeGenericType(typeof(TRequest), typeof(TResponse));
            Expression behavior = Expression.New(closedBehaviorType);
            var nextHandler = Expression.Lambda<RequestPipelineContinuation<TResponse>>(current);

            current = Expression.Call(
                behavior,
                nameof(IPipelineBehavior<TRequest, TResponse>.Handle),
                typeArguments: null,
                serviceProviderParameter,
                requestParameter,
                nextHandler,
                cancellationTokenParameter);
        }

        var lambda = Expression.Lambda<CompiledPipeline<TRequest, TResponse>>(
            current,
            serviceProviderParameter,
            handlerParameter,
            requestParameter,
            cancellationTokenParameter);

        return lambda.Compile();
    }
}
