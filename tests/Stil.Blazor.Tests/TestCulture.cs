using System.Globalization;
using System.Runtime.CompilerServices;

namespace Stil.Blazor.Tests;

internal static class TestCulture
{
    /// <summary>
    /// The components' built-in texts follow the UI culture, so the tests run with English UI texts
    /// whatever the machine's language is. Tests of other languages set the culture themselves.
    /// </summary>
    [ModuleInitializer]
    internal static void UseEnglishTexts()
    {
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
    }
}
