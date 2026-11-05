namespace Stil.Blazor;

/// <summary>Encodes Code 128, EAN-13 and EAN-8 barcodes as a row of modules (true is a bar).</summary>
internal static class BarcodeEncoder
{
    // Code 128 bar and space widths of the symbol values 0–105; each adds up to 11 modules.
    private static readonly string[] Code128Patterns =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232",
    ];

    private const string Code128Stop = "2331112";
    private const int StartB = 104;
    private const int StartC = 105;
    private const int CodeB = 100;

    // EAN digit codes for the left half with odd parity (L); R is its complement and G is R reversed.
    private static readonly string[] EanL = ["0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011"];

    // Which of the six left digits of an EAN-13 use G, by the first digit.
    private static readonly string[] Ean13Parity = ["LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLG", "LGLGGL", "LGLGLG", "LGGLGL"];

    /// <summary>The modules of the code, and the text to print under it.</summary>
    public static (bool[] Modules, string Text) Encode(string value, StilBarcodeFormat format)
    {
        ArgumentNullException.ThrowIfNull(value);
        return format switch
        {
            StilBarcodeFormat.Ean13 => Ean(value, 13),
            StilBarcodeFormat.Ean8 => Ean(value, 8),
            _ => (Code128(value), value),
        };
    }

    /// <summary>The EAN check digit for the digits before it.</summary>
    public static int EanCheckDigit(ReadOnlySpan<char> digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            // From the right, the digits are weighted 3, 1, 3, …
            sum += (digits[digits.Length - 1 - i] - '0') * (i % 2 == 0 ? 3 : 1);
        }

        return (10 - sum % 10) % 10;
    }

    // Code set C (two digits per symbol) for text of four or more digits, otherwise code set B
    // (printable ASCII). An odd digit at the end switches to B.
    private static bool[] Code128(string value)
    {
        if (value.Length == 0 || value.Any(c => c < ' ' || c > '~'))
        {
            throw new ArgumentException("Code 128 takes printable ASCII characters (space to ~).", nameof(value));
        }

        var symbols = new List<int>();
        if (value.Length >= 4 && value.All(char.IsAsciiDigit))
        {
            symbols.Add(StartC);
            var i = 0;
            for (; i + 1 < value.Length; i += 2)
            {
                symbols.Add((value[i] - '0') * 10 + (value[i + 1] - '0'));
            }

            if (i < value.Length)
            {
                symbols.Add(CodeB);
                symbols.Add(value[i] - ' ');
            }
        }
        else
        {
            symbols.Add(StartB);
            symbols.AddRange(value.Select(c => c - ' '));
        }

        var checksum = symbols[0];
        for (var i = 1; i < symbols.Count; i++)
        {
            checksum += i * symbols[i];
        }

        symbols.Add(checksum % 103);

        var modules = new List<bool>();
        foreach (var symbol in symbols)
        {
            AddWidths(modules, Code128Patterns[symbol]);
        }

        AddWidths(modules, Code128Stop);
        return [.. modules];
    }

    private static void AddWidths(List<bool> modules, string widths)
    {
        for (var i = 0; i < widths.Length; i++)
        {
            // Bars and spaces alternate, starting with a bar.
            modules.AddRange(Enumerable.Repeat(i % 2 == 0, widths[i] - '0'));
        }
    }

    // EAN-13 takes 12 digits (the check digit is added) or 13 (it is checked); EAN-8 takes 7 or 8.
    private static (bool[] Modules, string Text) Ean(string value, int length)
    {
        var digits = value.Trim();
        if (!digits.All(char.IsAsciiDigit) || digits.Length != length && digits.Length != length - 1)
        {
            throw new ArgumentException($"EAN-{length} takes {length - 1} or {length} digits.", nameof(value));
        }

        var check = EanCheckDigit(digits.AsSpan(0, length - 1));
        if (digits.Length == length - 1)
        {
            digits += (char)('0' + check);
        }
        else if (digits[^1] - '0' != check)
        {
            throw new ArgumentException($"The check digit of {digits} should be {check}.", nameof(value));
        }

        var bits = new System.Text.StringBuilder("101");
        var half = length == 13 ? 6 : 4;
        var left = length == 13 ? digits.Substring(1, 6) : digits[..4];
        var right = digits[^half..];
        var parity = length == 13 ? Ean13Parity[digits[0] - '0'] : "LLLL";
        for (var i = 0; i < half; i++)
        {
            var l = EanL[left[i] - '0'];
            bits.Append(parity[i] == 'L' ? l : new string(Complement(l).Reverse().ToArray()));
        }

        bits.Append("01010");
        foreach (var digit in right)
        {
            bits.Append(Complement(EanL[digit - '0']));
        }

        bits.Append("101");
        return (bits.ToString().Select(c => c == '1').ToArray(), digits);
    }

    private static string Complement(string bits) => new([.. bits.Select(c => c == '1' ? '0' : '1')]);
}
