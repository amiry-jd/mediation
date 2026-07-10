# FAQ

## Why another mediator?

Mediation is intentionally small and focused on request/response dispatch with precompiled pipeline behavior delegates.

## Can handlers use dependency injection?

Yes. Handlers are registered as scoped services and resolved from the current scope.

## Can behaviors use dependency injection?

Behaviors are parameterless structs. Use the `IServiceProvider` parameter inside `Handle` when a behavior needs a service.

## Can multiple handlers handle one request?

No. A concrete request type has one registered pipeline. Duplicate registrations fail during configuration.

## Is assembly scanning required?

No. Explicit registration is supported and recommended for maximum predictability.
