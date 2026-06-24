using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Mediation;

public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>();

public delegate ValueTask<TResponse>
    CompiledPipeline<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        IRequestHandler<TRequest, TResponse> handler,
        TRequest request,
        CancellationToken ct)
    where TRequest : IRequest<TResponse>;

internal static class RequestSlot<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static CompiledPipeline<TRequest, TResponse>? Pipeline;
}



internal static class PrebuiltPipelineCompiler
{
    [RequiresUnreferencedCode("Calls System.Linq.Expressions.Expression.Call(Expression, String, Type[], params Expression[])")]
    [RequiresDynamicCode("Calls System.Linq.Expressions.Expression.Call(Expression, String, Type[], params Expression[])")]
    public static CompiledPipeline<TRequest, TResponse>
        Compile<TRequest, TResponse>(
            Type[] behaviorTypes)
        where TRequest : IRequest<TResponse>
    {
        var spParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

        var handlerParam =
            Expression.Parameter(
                typeof(IRequestHandler<TRequest, TResponse>),
                "handler");

        var requestParam =
            Expression.Parameter(typeof(TRequest), "request");

        var ctParam =
            Expression.Parameter(typeof(CancellationToken), "ct");

        // handler.Handle(request, ct)
        Expression current =
            Expression.Call(
                handlerParam,
                nameof(IRequestHandler<,>.Handle),
                null,
                requestParam,
                ctParam);

        // wrap behaviors
        for (int i = behaviorTypes.Length - 1; i >= 0; i--)
        {
            var behaviorType = behaviorTypes[i];

            //var factoryType = typeof(BehaviorFactory<>).MakeGenericType(behaviorType);

            //var resolveField = factoryType.GetField("Resolve")!;

            //var resolver = Expression.Field(null, resolveField);

            //var behaviorExpr = Expression.Invoke(resolver, spParam);

            var behaviorGenericType = behaviorType.MakeGenericType(typeof(TRequest), typeof(TResponse));
            
            Expression b = Expression.New(behaviorGenericType);

            var next = Expression.Lambda<RequestHandlerDelegate<TResponse>>(current);

            current =
               Expression.Call(b, nameof(IPipelineBehavior<,>.Handle), null, spParam, requestParam, next, ctParam);
        }
        
        var lambda =
          Expression.Lambda<
              CompiledPipeline<TRequest, TResponse>>(
                  current,
                  spParam,
                  handlerParam,
                  requestParam,
                  ctParam);

        return lambda.Compile();
    }
}