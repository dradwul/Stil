using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;

namespace Stil.Docs;

/// <summary>A component parameter, for the API tables.</summary>
public sealed record ApiParameter(string Name, string Type, string? Default, string? Description, bool Required);

/// <summary>
/// Lists a component's parameters with reflection, and their descriptions from the libraries' XML
/// documentation (embedded at build time), so the API tables never drift from the code.
/// </summary>
public static partial class ApiDocs
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<ApiParameter>> Cache = new();
    private static readonly Lazy<Dictionary<string, string>> Summaries = new(LoadSummaries);

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public static IReadOnlyList<ApiParameter> Parameters(Type component) => Cache.GetOrAdd(component, Read);

    private static IReadOnlyList<ApiParameter> Read(Type component)
    {
        var instance = TryCreate(component);
        return component.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() is { CaptureUnmatchedValues: false })
            .Select(p => new ApiParameter(
                p.Name,
                FriendlyName(p.PropertyType, p),
                DefaultOf(p, instance),
                Summary(p),
                p.GetCustomAttribute<EditorRequiredAttribute>() is not null))
            .ToList();
    }

    // Generic components are created with object (or a constraint-free type) to read defaults.
    private static object? TryCreate(Type component)
    {
        try
        {
            var type = component.IsGenericTypeDefinition
                ? component.MakeGenericType(component.GetGenericArguments().Select(_ => typeof(object)).ToArray())
                : component;
            return Activator.CreateInstance(type);
        }
        catch (Exception ex) when (ex is ArgumentException or MissingMethodException or TargetInvocationException or NotSupportedException)
        {
            return null;
        }
    }

    private static string? DefaultOf(PropertyInfo property, object? instance)
    {
        if (instance is null)
        {
            return null;
        }

        object? value;
        try
        {
            value = instance.GetType().GetProperty(property.Name)?.GetValue(instance);
        }
        catch (TargetInvocationException)
        {
            return null;
        }

        return value switch
        {
            null => null,
            bool b => b ? "true" : null,
            string s when s.Length == 0 => null,
            string s => $"\"{s}\"",
            Enum e => $"{e.GetType().Name}.{e}",
            int or long or float or double or decimal when Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) == 0 => null,
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => null,
        };
    }

    private static string? Summary(PropertyInfo property)
    {
        var declaring = property.DeclaringType!;
        if (declaring.IsGenericType)
        {
            declaring = declaring.GetGenericTypeDefinition();
        }

        return Summaries.Value.TryGetValue($"P:{declaring.FullName}.{property.Name}", out var text) ? text : null;
    }

    private static Dictionary<string, string> LoadSummaries()
    {
        var summaries = new Dictionary<string, string>(StringComparer.Ordinal);
        var assembly = typeof(ApiDocs).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.StartsWith("Docs/", StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var document = XDocument.Load(stream);
            foreach (var member in document.Descendants("member"))
            {
                var summary = member.Element("summary");
                if (member.Attribute("name")?.Value is { } id && summary is not null)
                {
                    summaries[id] = Text(summary);
                }
            }
        }

        return summaries;
    }

    // Flattens <c>, <see cref> and line breaks into plain text.
    private static string Text(XElement element)
    {
        var text = new StringBuilder();
        foreach (var node in element.Nodes())
        {
            text.Append(node switch
            {
                XText t => t.Value,
                XElement { Name.LocalName: "see" } see => see.Attribute("cref")?.Value.Split('.', ':').Last() ?? see.Value,
                XElement e => e.Value,
                _ => "",
            });
        }

        return Whitespace().Replace(text.ToString(), " ").Trim();
    }

    /// <summary>A C#-like type name, for example <c>EventCallback&lt;bool&gt;</c> or <c>string?</c>.</summary>
    public static string FriendlyName(Type type, PropertyInfo? property = null)
    {
        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable is not null)
        {
            return FriendlyName(nullable) + "?";
        }

        var name = type switch
        {
            _ when type == typeof(string) => "string",
            _ when type == typeof(bool) => "bool",
            _ when type == typeof(int) => "int",
            _ when type == typeof(long) => "long",
            _ when type == typeof(float) => "float",
            _ when type == typeof(double) => "double",
            _ when type == typeof(decimal) => "decimal",
            _ when type == typeof(object) => "object",
            _ when type.IsGenericParameter => type.Name,
            _ when type.IsArray => FriendlyName(type.GetElementType()!) + "[]",
            _ when type.IsGenericType => $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(t => FriendlyName(t)))}>",
            _ => type.Name,
        };

        if (property is not null && !type.IsValueType && type != typeof(RenderFragment) && !type.IsGenericType
            && new NullabilityInfoContext().Create(property).ReadState == NullabilityState.Nullable)
        {
            name += "?";
        }

        return name;
    }
}
