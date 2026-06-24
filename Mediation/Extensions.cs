using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Mediation;

public sealed class MediatorConfigurationGroup
{
    private readonly Type[] behaviorTypes;
    private readonly IServiceCollection services;

    [RequiresUnreferencedCode("Calls System.Type.GetConstructors().")]
    internal MediatorConfigurationGroup(IServiceCollection services, Type[] behaviorTypes)
    {
        for (var i = 0; i < behaviorTypes.Length; i++)
        {
            var b = behaviorTypes[i];
            if (b.IsClass || b.IsInterface || b.IsEnum || !b.IsValueType || !b.IsSealed)
                throw new ArgumentException("The Mediation library only accepts 'struct's as behavior!");
            var ctors = b.GetConstructors();
            if (ctors.Length > 1 || ctors.Any(c => c.GetParameters().Length > 0))
                throw new ArgumentException("The Mediation library only accepts behaviors with default constructor with no parameter!");
        }
        this.behaviorTypes = behaviorTypes;
        this.services = services;
    }

    [RequiresUnreferencedCode("Calls System.Linq.Expressions.Expression.Call(Expression, String, Type[], params Expression[]).")]
    [RequiresDynamicCode("Calls System.Linq.Expressions.Expression.Call(Expression, String, Type[], params Expression[]).")]
    public MediatorConfigurationGroup Register<
            TReq,
            TRes,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TReqHandler>()
        where TReqHandler : class, IRequestHandler<TReq, TRes>
        where TReq : IRequest<TRes>
    {
        services.AddScoped<IRequestHandler<TReq, TRes>, TReqHandler>();

        var pipeline = PrebuiltPipelineCompiler.Compile<TReq, TRes>(behaviorTypes);

        RequestSlot<TReq, TRes>.Pipeline = pipeline;

        return this;
    }

    [RequiresUnreferencedCode("")]
    [RequiresDynamicCode("")]
    public MediatorConfigurationGroup RegisterForAssemblies(Assembly assembly, params Assembly[] assemblies)
    {
        assemblies = [assembly, ..assemblies];
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(static t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Select(x => new
            {
                HandlerType = x.Type,
                RequestType = x.Interface.GetGenericArguments()[0],
                ResponseType = x.Interface.GetGenericArguments()[1]
            })
            .ToList();
        foreach (var type in types)
        {
            var method = GetType().GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(type.RequestType, type.ResponseType, type.HandlerType);
            genericMethod.Invoke(this, null);
        }
        return this;
    }
}

public sealed class MediatorConfiguration
{
    private readonly IServiceCollection services;

    internal MediatorConfiguration(IServiceCollection services) 
        => this.services = services;

    [RequiresUnreferencedCode("Calls Mediation.MediatorConfigurationGroup.MediatorConfigurationGroup(IServiceCollection, Type[]).")]
    public MediatorConfigurationGroup WithBehaviors(params Type[] behaviorTypes)
        => new(services, behaviorTypes);

    [RequiresUnreferencedCode("Calls Mediation.MediatorConfigurationGroup.MediatorConfigurationGroup(IServiceCollection, Type[]).")]
    public MediatorConfigurationGroup WithNoBehavior()
        => new(services, []);
}

public static class Extensions
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Action<MediatorConfiguration> configBuilder)
    {
        services.AddScoped<IMediator, Mediator>();
        var config = new MediatorConfiguration(services);
        configBuilder(config);
        return services;
    }
}