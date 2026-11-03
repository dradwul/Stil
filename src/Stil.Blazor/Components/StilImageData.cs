namespace Stil.Blazor.Components;

/// <summary>
/// An image made in the browser by <see cref="StilImageCropper"/> or <see cref="StilCameraCapture"/>,
/// as a data URL (to show it at once with &lt;img src&gt;) and as bytes (to save or upload it).
/// </summary>
public sealed class StilImageData
{
    public StilImageData(string dataUrl, int width, int height)
    {
        ArgumentException.ThrowIfNullOrEmpty(dataUrl);

        var comma = dataUrl.IndexOf(',');
        if (!dataUrl.StartsWith("data:", StringComparison.Ordinal) || comma < 0 || !dataUrl[..comma].EndsWith(";base64", StringComparison.Ordinal))
        {
            throw new FormatException("Expected a base64 data URL.");
        }

        DataUrl = dataUrl;
        ContentType = dataUrl[5..comma].Replace(";base64", "", StringComparison.Ordinal);
        Bytes = Convert.FromBase64String(dataUrl[(comma + 1)..]);
        Width = width;
        Height = height;
    }

    /// <summary>The image as a data URL, for an &lt;img src&gt;.</summary>
    public string DataUrl { get; }

    /// <summary>For example image/png or image/jpeg.</summary>
    public string ContentType { get; }

    /// <summary>The encoded image file.</summary>
    public byte[] Bytes { get; }

    public int Width { get; }

    public int Height { get; }

    /// <summary>A file name with the right extension, for example "avatar.png".</summary>
    public string FileName(string name) => $"{name}.{(ContentType == "image/jpeg" ? "jpg" : ContentType.Split('/')[^1])}";

    /// <summary>The bytes as a stream, for example for a storage SDK.</summary>
    public Stream OpenReadStream() => new MemoryStream(Bytes, writable: false);
}
