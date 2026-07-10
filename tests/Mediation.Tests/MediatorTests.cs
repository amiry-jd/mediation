using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Xunit;

namespace Mediation.Tests;

public sealed class MediatorTests
{
    [Fact]
    public async Task Send_dispatches_request_to_registered_handler()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<PingRequest, PingResponse, PingHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new PingRequest("hello"), CancellationToken.None);

        response.Message.Should().Be("Handled: hello");
    }

    [Fact]
    public async Task Send_executes_behaviors_in_configured_order()
    {
        RecordingBehavior<PingRequest, PingResponse>.Events.Clear();
        AuthorizationBehavior<PingRequest, PingResponse>.Events.Clear();

        using var provider = CreateProvider(static configuration =>
            configuration
                .WithBehaviors(
                    typeof(RecordingBehavior<,>),
                    typeof(AuthorizationBehavior<,>))
                .Register<PingRequest, PingResponse, PingHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new PingRequest("pipeline"), CancellationToken.None);

        RecordingBehavior<PingRequest, PingResponse>.Events.Should().Equal(
            "recording-before",
            "authorization-before",
            "authorization-after",
            "recording-after");
    }

    [Fact]
    public async Task Send_uses_scoped_handler_instances()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<CountingRequest, int, CountingHandler>());

        var firstScopeResult = await SendCountingRequest(provider);
        var secondScopeResult = await SendCountingRequest(provider);

        firstScopeResult.Should().Be(1);
        secondScopeResult.Should().Be(1);
    }

    [Fact]
    public async Task Send_flows_cancellation_token_to_handler()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<CancellableRequest, CancellationToken, CancellableHandler>());

        using var cancellationTokenSource = new CancellationTokenSource();
        var mediator = provider.GetRequiredService<IMediator>();

        var token = await mediator.Send(
            new CancellableRequest(),
            cancellationTokenSource.Token);

        token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Send_propagates_handler_exceptions()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<ThrowingRequest, string, ThrowingHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        var act = async () => await mediator.Send(new ThrowingRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("handler failed");
    }

    [Fact]
    public async Task Send_supports_reference_type_null_response()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<NullableResponseRequest, string?, NullableResponseHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new NullableResponseRequest(), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Send_is_safe_for_concurrent_requests()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<CountingRequest, int, CountingHandler>());

        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var tasks = Enumerable
            .Range(0, 128)
            .Select(_ => mediator.Send(new CountingRequest(), CancellationToken.None).AsTask());

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyHaveUniqueItems();
        results.Should().HaveCount(128);
    }

    [Fact]
    public void Send_throws_when_request_is_null()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<PingRequest, PingResponse, PingHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        var act = () => mediator.Send<PingResponse>(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public void Send_throws_when_request_is_not_registered()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().Register<PingRequest, PingResponse, PingHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        var act = () => mediator.Send(new MissingRequest());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MissingRequest*");
    }

    [Fact]
    public void AddMediator_throws_when_services_is_null()
    {
        IServiceCollection services = null!;

        var act = () => services.AddMediator(static _ => { });

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddMediator_throws_when_configure_is_null()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configure");
    }

    [Fact]
    public void Register_throws_for_duplicate_request_registration()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
        {
            configuration.WithNoBehavior().Register<PingRequest, PingResponse, PingHandler>();
            configuration.WithNoBehavior().Register<PingRequest, PingResponse, AlternatePingHandler>();
        });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PingRequest*already*");
    }

    [Fact]
    public void WithBehaviors_throws_when_behavior_is_null()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
            configuration.WithBehaviors(null!));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("behaviorTypes");
    }

    [Fact]
    public void WithBehaviors_throws_for_closed_behavior_type()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
            configuration.WithBehaviors(typeof(RecordingBehavior<PingRequest, PingResponse>)));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*open generic*");
    }

    [Fact]
    public void WithBehaviors_throws_for_class_behavior_type()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
            configuration.WithBehaviors(typeof(ClassBehavior<,>)));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*struct*");
    }

    [Fact]
    public void WithBehaviors_throws_for_behavior_with_constructor_parameters()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
            configuration.WithBehaviors(typeof(BehaviorWithConstructor<,>)));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*parameterless*");
    }

    [Fact]
    public async Task RegisterForAssemblies_discovers_handlers()
    {
        using var provider = CreateProvider(static configuration =>
            configuration.WithNoBehavior().RegisterForAssemblies(typeof(PingHandler).Assembly));

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new PingRequest("scan"), CancellationToken.None);

        response.Message.Should().Be("Handled: scan");
    }

    [Fact]
    public void RegisterForAssemblies_throws_when_assembly_is_null()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMediator(static configuration =>
            configuration.WithNoBehavior().RegisterForAssemblies(null!));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assembly");
    }

    [Fact]
    public async Task Send_can_use_substituted_handler_dependency()
    {
        var formatter = Substitute.For<IMessageFormatter>();
        formatter.Format("substitute").Returns("mocked");

        var services = new ServiceCollection();
        services.AddSingleton(formatter);
        services.AddMediator(static configuration =>
            configuration.WithNoBehavior().Register<FormattedRequest, PingResponse, FormattedHandler>());

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new FormattedRequest("substitute"), CancellationToken.None);

        response.Message.Should().Be("mocked");
        formatter.Received(1).Format("substitute");
    }

    private static ServiceProvider CreateProvider(Action<MediatorConfiguration> configure)
    {
        var services = new ServiceCollection();
        services.AddMediator(configure);
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static async Task<int> SendCountingRequest(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(new CountingRequest(), CancellationToken.None);
    }

    private sealed record PingRequest(string Message) : IRequest<PingResponse>;

    private sealed record PingResponse(string Message);

    private sealed record MissingRequest : IRequest<string>;

    private sealed record CountingRequest : IRequest<int>;

    private sealed record CancellableRequest : IRequest<CancellationToken>;

    private sealed record ThrowingRequest : IRequest<string>;

    private sealed record NullableResponseRequest : IRequest<string?>;

    private sealed record FormattedRequest(string Message) : IRequest<PingResponse>;

    private interface IMessageFormatter
    {
        string Format(string message);
    }

    private sealed class PingHandler : IRequestHandler<PingRequest, PingResponse>
    {
        public ValueTask<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken)
            => ValueTask.FromResult(new PingResponse($"Handled: {request.Message}"));
    }

    private sealed class AlternatePingHandler : IRequestHandler<PingRequest, PingResponse>
    {
        public ValueTask<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken)
            => ValueTask.FromResult(new PingResponse($"Alternate: {request.Message}"));
    }

    private sealed class CountingHandler : IRequestHandler<CountingRequest, int>
    {
        private int count;

        public ValueTask<int> Handle(CountingRequest request, CancellationToken cancellationToken)
            => ValueTask.FromResult(Interlocked.Increment(ref count));
    }

    private sealed class CancellableHandler : IRequestHandler<CancellableRequest, CancellationToken>
    {
        public ValueTask<CancellationToken> Handle(
            CancellableRequest request,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(cancellationToken);
    }

    private sealed class ThrowingHandler : IRequestHandler<ThrowingRequest, string>
    {
        public ValueTask<string> Handle(ThrowingRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("handler failed");
    }

    private sealed class NullableResponseHandler : IRequestHandler<NullableResponseRequest, string?>
    {
        public ValueTask<string?> Handle(NullableResponseRequest request, CancellationToken cancellationToken)
            => ValueTask.FromResult<string?>(null);
    }

    private sealed class FormattedHandler(IMessageFormatter formatter) : IRequestHandler<FormattedRequest, PingResponse>
    {
        public ValueTask<PingResponse> Handle(
            FormattedRequest request,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(new PingResponse(formatter.Format(request.Message)));
    }

    private readonly struct RecordingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> Events { get; } = [];

        public async ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
        {
            Events.Add("recording-before");
            var response = await nextHandler().ConfigureAwait(false);
            Events.Add("recording-after");
            return response;
        }
    }

    private readonly struct AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> Events => RecordingBehavior<TRequest, TResponse>.Events;

        public async ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
        {
            Events.Add("authorization-before");
            var response = await nextHandler().ConfigureAwait(false);
            Events.Add("authorization-after");
            return response;
        }
    }

    private sealed class ClassBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
            => nextHandler();
    }

    private readonly struct BehaviorWithConstructor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public BehaviorWithConstructor(string value)
            => Value = value;

        public string Value { get; }

        public ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
            => nextHandler();
    }
}
