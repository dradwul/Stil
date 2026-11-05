using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilCodesTests
{
    [Fact]
    public void QrCodeHasFinderPatternsInThreeCorners()
    {
        // Five alphanumeric characters fit in version 1: 21 × 21 modules.
        var modules = StilCodes.QrModules("HELLO");

        Assert.Equal(21, modules.GetLength(0));
        foreach (var (row, column) in new[] { (0, 0), (0, 14), (14, 0) })
        {
            Assert.True(modules[row, column]);
            Assert.True(modules[row + 6, column + 6]);
            Assert.False(modules[row + 1, column + 1]);
            Assert.True(modules[row + 3, column + 3]);
        }

        // The light separator around the top-left finder.
        Assert.False(modules[7, 7]);
    }

    [Fact]
    public void MoreErrorCorrectionMakesALargerCode()
    {
        const string url = "https://example.com/visits/42?badge=7";

        Assert.True(StilCodes.QrModules(url, StilQrErrorCorrection.High).GetLength(0) > StilCodes.QrModules(url, StilQrErrorCorrection.Low).GetLength(0));
    }

    [Fact]
    public void TextTooLongForAQrCodeThrows()
    {
        Assert.Throws<ArgumentException>(() => StilCodes.QrModules(new string('x', 3000), StilQrErrorCorrection.High));
    }

    [Theory]
    [InlineData("590123412345", 7)]
    [InlineData("400638133393", 1)]
    [InlineData("9638507", 4)]
    public void EanCheckDigit(string digits, int expected)
    {
        Assert.Equal(expected, StilCodes.EanCheckDigit(digits));
    }

    [Theory]
    [InlineData("590123412345", StilBarcodeFormat.Ean13, 95)]
    [InlineData("5901234123457", StilBarcodeFormat.Ean13, 95)]
    [InlineData("9638507", StilBarcodeFormat.Ean8, 67)]
    // Start, three characters, check and the 13-module stop.
    [InlineData("ABC", StilBarcodeFormat.Code128, 5 * 11 + 13)]
    // Code set C packs the digits in pairs: start, three pairs, CODE B, the last digit, check, stop.
    [InlineData("1234567", StilBarcodeFormat.Code128, 7 * 11 + 13)]
    public void BarcodeHasTheFormatsWidth(string value, StilBarcodeFormat format, int width)
    {
        var modules = StilCodes.BarcodeModules(value, format);

        Assert.Equal(width, modules.Length);
        Assert.True(modules[0]);
        Assert.True(modules[^1]);
    }

    [Theory]
    [InlineData("5901234123458", StilBarcodeFormat.Ean13)]
    [InlineData("12AB", StilBarcodeFormat.Ean8)]
    [InlineData("Åsa", StilBarcodeFormat.Code128)]
    [InlineData("", StilBarcodeFormat.Code128)]
    public void InvalidBarcodeValueThrows(string value, StilBarcodeFormat format)
    {
        Assert.Throws<ArgumentException>(() => StilCodes.BarcodeModules(value, format));
    }

    [Fact]
    public void SvgUsesTheGivenColors()
    {
        var svg = StilCodes.QrSvg("HELLO", color: "#123456", background: "#fedcba");

        Assert.StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 29 29\"", svg);
        Assert.Contains("fill=\"#fedcba\"", svg);
        Assert.Contains("<path fill=\"#123456\"", svg);
    }

    [Fact]
    public void QrCodeComponentRendersAnImageNamedByItsValue()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilQrCode>(parameters => parameters
            .Add(component => component.Value, "HELLO")
            .Add(component => component.Size, "8rem"));

        var code = cut.Find(".stil-qr-code");
        Assert.Equal("img", code.GetAttribute("role"));
        Assert.Equal("HELLO", code.GetAttribute("aria-label"));
        Assert.Equal("width: 8rem", code.GetAttribute("style"));
        Assert.Equal("0 0 29 29", cut.Find("svg").GetAttribute("viewBox"));
        Assert.Equal("currentColor", cut.Find("path").GetAttribute("fill"));
        Assert.Empty(cut.FindAll("rect"));
    }

    [Fact]
    public void BarcodeComponentPrintsTheValueWithItsCheckDigit()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilBarcode>(parameters => parameters
            .Add(component => component.Value, "590123412345")
            .Add(component => component.Format, StilBarcodeFormat.Ean13)
            .Add(component => component.Label, "Product"));

        Assert.Equal("Product", cut.Find(".stil-barcode").GetAttribute("aria-label"));
        Assert.Equal("5901234123457", cut.Find("text").TextContent);
    }

    [Fact]
    public void InvalidValueShowsALocalizedMessage()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilBarcode>(parameters => parameters
            .Add(component => component.Value, "not digits")
            .Add(component => component.Format, StilBarcodeFormat.Ean8));

        Assert.Empty(cut.FindAll("svg"));
        Assert.Equal("This code cannot be shown.", cut.Find(".stil-code-invalid").TextContent);
    }
}
