using System.Diagnostics.CodeAnalysis;

namespace Mediation.SampleConsole;

[RequiresDynamicCode("")]
[RequiresUnreferencedCode("")]
public class Program
{
    private static void Main(string[] args)
    {
        Run(args);
    }

    private static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMediator(static config =>
        {
            config.WithBehaviors(
                    typeof(LoggingBehavior<,>),
                    typeof(AuthBehavior<,>)
                )
                //.Register<CreateUserRequest, CreateUserResponse, CreateUserRequestHandler>()
                .RegisterForAssemblies(typeof(CreateUserRequest).Assembly);
            //config.WithNoBehavior()
            //    .Register<CreateUserRequest, CreateUserResponse, CreateUserRequestHandler>();
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var m = sp.GetRequiredService<IMediator>();
            var req = new CreateUserRequest();
            var res = m.Send(req).GetAwaiter().GetResult();
            Console.WriteLine(res.GetType().Name);
        }

        app.Run();
    }
}