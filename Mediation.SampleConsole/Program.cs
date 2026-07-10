using System.Diagnostics.CodeAnalysis;

namespace Mediation.SampleConsole;

[RequiresDynamicCode("")]
[RequiresUnreferencedCode("")]
internal static class Program
{
    private static async Task Main(string[] args)
        => await Run(args).ConfigureAwait(false);

    private static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMediator(static config =>
        {
            config.WithBehaviors(
                    typeof(LoggingBehavior<,>),
                    typeof(AuthBehavior<,>)
                )
                .RegisterForAssemblies(typeof(CreateUserRequest).Assembly);
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var m = sp.GetRequiredService<IMediator>();
            var req = new CreateUserRequest();
            var response = await m.Send(req).ConfigureAwait(false);
            Console.WriteLine(response.GetType().Name);
        }

        await app.RunAsync().ConfigureAwait(false);
    }
}
