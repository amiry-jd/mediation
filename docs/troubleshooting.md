# Troubleshooting

## Missing Handler

Error: `No request handler has been registered`.

Fix: call `Register<TRequest,TResponse,THandler>()` or include the handler assembly in `RegisterForAssemblies`.

## Duplicate Handler

Error: `A request handler has already been registered`.

Fix: ensure each concrete request type is registered once.

## Behavior Type Rejected

Behaviors must be open generic structs:

```csharp
typeof(MyBehavior<,>)
```

Classes, closed generic types, and constructors with parameters are rejected.

## Trim or AOT Warnings

Reflection-based assembly scanning and expression compilation are dynamic-code paths. Prefer explicit registration and validate publish settings for trimmed applications.
