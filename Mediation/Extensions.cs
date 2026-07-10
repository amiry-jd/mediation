using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mediation;

/// <summary>
/// Configures request handler registrations for the mediator.
/// </summary>
public sealed class MediatorConfigurationGroup
{
    private readonly IReadOnlyList<Type> behaviorTypes;
    private readonly MediatorRegistry registry;
    private readonly IServiceCollection services;

    [RequiresUnreferencedCode("Validating behavior constructors uses reflection and is not compatible with trimming.")]
    internal MediatorConfigurationGroup(
        IServiceCollection services,
        MediatorRegistry registry,
        IReadOnlyList<Type> behaviorTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(behaviorTypes);

        foreach (var behaviorType in behaviorTypes)
        {
            ValidateBehaviorType(behaviorType);
        }

        this.behaviorTypes = behaviorTypes.ToArray();
        this.registry = registry;
        this.services = services;
    }

    /// <summary>
    /// Registers a request handler for a specific request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The request type handled by the handler.</typeparam>
    /// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
    /// <typeparam name="TRequestHandler">The concrete handler implementation type.</typeparam>
    /// <returns>The current configuration group.</returns>
    [RequiresUnreferencedCode("Compiling behavior pipelines uses reflection and is not compatible with trimming.")]
    [RequiresDynamicCode("Compiling behavior pipelines uses expression compilation.")]
    public MediatorConfigurationGroup Register<
        TRequest,
        TResponse,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRequestHandler>()
        where TRequest : IRequest<TResponse>
        where TRequestHandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, TRequestHandler>();
        registry.Add(PipelineCompiler.Compile<TRequest, TResponse>(behaviorTypes));

        return this;
    }

    /// <summary>
    /// Registers all concrete request handlers found in the supplied assemblies.
    /// </summary>
    /// <param name="assembly">The first assembly to scan.</param>
    /// <param name="assemblies">Additional assemblies to scan.</param>
    /// <returns>The current configuration group.</returns>
    [RequiresUnreferencedCode("Scanning assemblies and compiling behavior pipelines use reflection and are not compatible with trimming.")]
    [RequiresDynamicCode("Compiling behavior pipelines uses expression compilation.")]
    public MediatorConfigurationGroup RegisterForAssemblies(
        Assembly assembly,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(assemblies);

        var targetAssemblies = new Assembly[assemblies.Length + 1];
        targetAssemblies[0] = assembly;
        Array.Copy(assemblies, 0, targetAssemblies, 1, assemblies.Length);

        foreach (var handler in DiscoverHandlers(targetAssemblies))
        {
            RegisterHandler(handler);
        }

        return this;
    }

    private static IEnumerable<HandlerRegistration> DiscoverHandlers(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    if (!implementedInterface.IsGenericType ||
                        implementedInterface.GetGenericTypeDefinition() != typeof(IRequestHandler<,>))
                    {
                        continue;
                    }

                    var genericArguments = implementedInterface.GetGenericArguments();
                    yield return new HandlerRegistration(
                        type.AsType(),
                        genericArguments[0],
                        genericArguments[1]);
                }
            }
        }
    }

    [RequiresUnreferencedCode("Registering handler types discovered by reflection is not compatible with trimming.")]
    [RequiresDynamicCode("Registering handler types discovered by reflection requires MakeGenericMethod.")]
    private void RegisterHandler(HandlerRegistration handler)
    {
        var method = typeof(MediatorConfigurationGroup).GetMethod(
            nameof(Register),
            BindingFlags.Instance | BindingFlags.Public);

        var genericMethod = method!.MakeGenericMethod(
            handler.RequestType,
            handler.ResponseType,
            handler.HandlerType);

        genericMethod.Invoke(this, parameters: null);
    }

    private static void ValidateBehaviorType(Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(behaviorType);

        if (!behaviorType.IsGenericTypeDefinition || behaviorType.GetGenericArguments().Length != 2)
        {
            throw new ArgumentException(
                $"Behavior type '{behaviorType.FullName}' must be an open generic type with two generic parameters.",
                nameof(behaviorType));
        }

        if (!behaviorType.IsValueType || !behaviorType.IsSealed)
        {
            throw new ArgumentException(
                $"Behavior type '{behaviorType.FullName}' must be a readonly struct or struct.",
                nameof(behaviorType));
        }

        var constructors = behaviorType.GetConstructors();
        if (constructors.Any(constructor => constructor.GetParameters().Length > 0))
        {
            throw new ArgumentException(
                $"Behavior type '{behaviorType.FullName}' must expose only parameterless constructors.",
                nameof(behaviorType));
        }
    }

    private sealed record HandlerRegistration(
        Type HandlerType,
        Type RequestType,
        Type ResponseType);
}

/// <summary>
/// Configures mediator behavior groups.
/// </summary>
public sealed class MediatorConfiguration
{
    private readonly MediatorRegistry registry;
    private readonly IServiceCollection services;

    internal MediatorConfiguration(
        IServiceCollection services,
        MediatorRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(registry);

        this.services = services;
        this.registry = registry;
    }

    /// <summary>
    /// Creates a configuration group that applies the supplied behaviors to each registered handler.
    /// </summary>
    /// <param name="behaviorTypes">Open generic behavior types in the order they should execute.</param>
    /// <returns>A configuration group used to register request handlers.</returns>
    [RequiresUnreferencedCode("Validating behavior constructors uses reflection and is not compatible with trimming.")]
    public MediatorConfigurationGroup WithBehaviors(params Type[] behaviorTypes)
    {
        ArgumentNullException.ThrowIfNull(behaviorTypes);

        return new MediatorConfigurationGroup(services, registry, behaviorTypes);
    }

    /// <summary>
    /// Creates a configuration group without pipeline behaviors.
    /// </summary>
    /// <returns>A configuration group used to register request handlers.</returns>
    [RequiresUnreferencedCode("Configuration groups validate behavior constructors through reflection.")]
    public MediatorConfigurationGroup WithNoBehavior()
        => new(services, registry, []);
}

/// <summary>
/// Provides extension methods for registering the mediator with dependency injection.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds mediator services and configured request handlers to a service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">The mediator configuration callback.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null" />.</exception>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Action<MediatorConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var registry = new MediatorRegistry();
        services.TryAddSingleton(registry);
        services.TryAddScoped<IMediator, Mediator>();

        configure(new MediatorConfiguration(services, registry));

        return services;
    }
}
