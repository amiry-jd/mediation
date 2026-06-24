using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Mediation;

public interface IPipelineBehavior<TRequest, TResponse>
{
    ValueTask<TResponse> Handle(
        IServiceProvider serviceProvider,
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct);
}

public interface IMediator
{
    [RequiresDynamicCode("Calls dynamic code")]
    ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken ct = default
    );
}

public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Delegate> _cache = new();

    [RequiresDynamicCode("Calls Mediation.Mediator.GetCall<TResponse>(IRequest<TResponse>)")]
    public ValueTask<TResponse> Send<TResponse>(
          IRequest<TResponse> request,
          CancellationToken ct = default)
    {
        Delegate call = GetCall(request);
        var r = call.DynamicInvoke(this, request, ct);
        if (r is not null)
            return (ValueTask<TResponse>)r;
        return default;
    }

    [RequiresDynamicCode("Calls System.Linq.Expressions.Expression.Lambda(Expression, bool, params ParameterExpression[]?)")]
    private Delegate GetCall<TResponse>(IRequest<TResponse> request)
    {
        var reqType = request.GetType();
        return _cache.GetOrAdd(reqType, static (reqType, resType) =>
        {
            var thisParam = Expression.Parameter(typeof(Mediator), "t");
            var reqParam = Expression.Parameter(reqType, "request");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");
            var method = typeof(Mediator)
                .GetMethod(nameof(SendInternal), BindingFlags.Instance | BindingFlags.NonPublic);
            var methodInfo = method!.MakeGenericMethod(reqType, resType);
            Expression call = Expression.Call(thisParam, methodInfo, reqParam, ctParam);
            var lambda = Expression.Lambda(call, false, thisParam, reqParam, ctParam);
            return lambda.Compile();
        }, typeof(TResponse));
    }

    private ValueTask<TResponse> SendInternal<TRequest, TResponse>(
        TRequest request,
        CancellationToken ct)
        where TRequest : IRequest<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        var pipeline = RequestSlot<TRequest, TResponse>.Pipeline!;

        return pipeline(serviceProvider, handler, request, ct);
    }
}
