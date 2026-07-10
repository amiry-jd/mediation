# Architecture

Mediation is intentionally small.

## Components

- `IRequest<TResponse>` marks request messages.
- `IRequestHandler<TRequest,TResponse>` handles a request.
- `IMediator` dispatches requests.
- `MediatorRegistry` stores compiled request pipelines.
- `PipelineCompiler` composes handlers and behaviors.
- `IPipelineBehavior<TRequest,TResponse>` wraps cross-cutting behavior.

## Flow

1. Application calls `services.AddMediator(...)`.
2. Configuration registers handlers and compiles pipelines.
3. Application resolves `IMediator` from a scope.
4. `Send` looks up the concrete request type.
5. The typed pipeline resolves the scoped handler and invokes behaviors in order.

## Design Principles

- Keep public API minimal.
- Fail fast on duplicate or missing registrations.
- Avoid late-bound `DynamicInvoke` on hot dispatch paths.
- Keep behavior construction simple and allocation-conscious.
- Document reflection-based paths for trim/AOT-sensitive consumers.
