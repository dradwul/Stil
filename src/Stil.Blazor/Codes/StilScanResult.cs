namespace Stil.Blazor;

/// <summary>Where a scanned code came from.</summary>
public enum StilScanSource
{
    /// <summary>The camera, through <c>StilScanner</c>.</summary>
    Camera,

    /// <summary>A handheld USB or Bluetooth scanner typing into a <c>StilScanInput</c>.</summary>
    Scanner,

    /// <summary>A person typing the code and pressing Enter.</summary>
    Keyboard,
}

/// <summary>A scanned code.</summary>
/// <param name="Value">The text in the code.</param>
/// <param name="Format">The format the camera saw, as the browser names it (for example <c>qr_code</c>, <c>code_128</c> or <c>ean_13</c>); null for a scanner or the keyboard.</param>
/// <param name="Source">Where the code came from.</param>
public sealed record StilScanResult(string Value, string? Format, StilScanSource Source);
