using System.Collections.Concurrent;
using System.Reflection;

namespace Stil.Docs;

/// <summary>Reads the embedded .razor source of an example component.</summary>
public static class ExampleSource
{
    private static readonly Assembly Assembly = typeof(ExampleSource).Assembly;
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    // Resource names use the folder separator of the machine that built the app.
    private static readonly Dictionary<string, string> Resources = Assembly.GetManifestResourceNames()
        .ToDictionary(name => name.Replace('\\', '/'), name => name, StringComparer.OrdinalIgnoreCase);

    public static string Get(Type example) => Cache.GetOrAdd(example, Read);

    private static string Read(Type example)
    {
        // Stil.Docs.Examples.Buttons.Variants -> Examples/Buttons/Variants.razor
        var relative = example.FullName!["Stil.Docs.".Length..].Replace('.', '/') + ".razor";
        if (!Resources.TryGetValue(relative, out var name))
        {
            return $"// Source not found: {relative}";
        }

        using var stream = Assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n").Trim();
    }
}
