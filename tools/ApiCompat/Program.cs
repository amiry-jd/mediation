using System.Reflection;

using PublicApiGenerator;

namespace ApiCompat;

public static class Program
{
    public static int Main(string[] args)
    {
        var repositoryRoot = args.Length > 0
            ? Path.GetFullPath(args[0])
            : ResolveRepositoryRoot();

        var baselinePath = Path.Combine(repositoryRoot, "Mediation", "PublicAPI.Shipped.txt");
        var unshippedPath = Path.Combine(repositoryRoot, "Mediation", "PublicAPI.Unshipped.txt");
        string outputPath = Path.Combine(repositoryRoot, "artifacts", "api", "Mediation.PublicAPI.Current.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        string api = typeof(Mediation.IMediator)
            .Assembly
            .GeneratePublicApi(new ApiGeneratorOptions
            {
                IncludeAssemblyAttributes = false,
                IncludeNullableAttributes = false
            });

        File.WriteAllText(outputPath, api);

        string[] baseline = ReadApiLines(baselinePath)
            .Concat(ReadApiLines(unshippedPath))
            .ToArray();
        string[] current = Normalize(api).ToArray();

        string[] missing = baseline.Except(current, StringComparer.Ordinal).ToArray();
        string[] additions = current.Except(baseline, StringComparer.Ordinal).ToArray();
        if (missing.Length == 0 && additions.Length == 0)
        {
            Console.WriteLine($"Public API compatibility passed. Current API written to {outputPath}.");
            return 0;
        }

        if (missing.Length > 0)
        {
            Console.Error.WriteLine("Public API compatibility failed. Missing baseline members:");
            foreach (string member in missing)
            {
                Console.Error.WriteLine($"  {member}");
            }
        }

        if (additions.Length > 0)
        {
            Console.Error.WriteLine("Public API compatibility failed. Unapproved new public members:");
            foreach (string member in additions)
            {
                Console.Error.WriteLine($"  {member}");
            }
        }

        return 1;
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Mediation.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private static IEnumerable<string> ReadApiLines(string path)
        => File.Exists(path)
            ? Normalize(File.ReadAllText(path))
            : [];

    private static IEnumerable<string> Normalize(string content)
        => content
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !line.StartsWith("#", StringComparison.Ordinal));
}
