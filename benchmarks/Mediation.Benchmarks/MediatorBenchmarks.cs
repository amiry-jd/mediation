using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.DependencyInjection;

namespace Mediation.Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IMediator directMediator = null!;
    private IMediator pipelineMediator = null!;

    [GlobalSetup]
    public void Setup()
    {
        directMediator = BuildMediator(static configuration =>
            configuration.WithNoBehavior().Register<BenchmarkRequest, BenchmarkResponse, BenchmarkHandler>());

        pipelineMediator = BuildMediator(static configuration =>
            configuration
                .WithBehaviors(typeof(NoOpBehavior<,>), typeof(NoOpBehavior2<,>))
                .Register<BenchmarkRequest, BenchmarkResponse, BenchmarkHandler>());
    }

    [Benchmark(Baseline = true)]
    public ValueTask<BenchmarkResponse> SendWithoutBehaviors()
        => directMediator.Send(new BenchmarkRequest(42));

    [Benchmark]
    public ValueTask<BenchmarkResponse> SendWithTwoBehaviors()
        => pipelineMediator.Send(new BenchmarkRequest(42));

    private static IMediator BuildMediator(Action<MediatorConfiguration> configure)
    {
        var services = new ServiceCollection();
        services.AddMediator(configure);
        return services.BuildServiceProvider(validateScopes: true).GetRequiredService<IMediator>();
    }

    public readonly record struct BenchmarkRequest(int Value) : IRequest<BenchmarkResponse>;

    public readonly record struct BenchmarkResponse(int Value);

    private sealed class BenchmarkHandler : IRequestHandler<BenchmarkRequest, BenchmarkResponse>
    {
        public ValueTask<BenchmarkResponse> Handle(
            BenchmarkRequest request,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(new BenchmarkResponse(request.Value + 1));
    }

    private readonly struct NoOpBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
            => nextHandler();
    }

    private readonly struct NoOpBehavior2<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public ValueTask<TResponse> Handle(
            IServiceProvider serviceProvider,
            TRequest request,
            RequestPipelineContinuation<TResponse> nextHandler,
            CancellationToken cancellationToken)
            => nextHandler();
    }
}
