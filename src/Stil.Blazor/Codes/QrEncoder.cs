using System.Text;

namespace Stil.Blazor;

/// <summary>
/// Encodes text as a QR code (ISO/IEC 18004, model 2, versions 1–40). It picks the numeric,
/// alphanumeric or byte (UTF-8) mode for the whole text, the smallest version that fits, and the
/// mask with the lowest penalty.
/// </summary>
internal static class QrEncoder
{
    private const string Alphanumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

    // Per error correction level (L, M, Q, H) and version (index 0 unused).
    private static readonly int[][] EccCodewordsPerBlock =
    [
        [-1, 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],
        [-1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28],
        [-1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],
        [-1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30],
    ];

    private static readonly int[][] ErrorCorrectionBlocks =
    [
        [-1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25],
        [-1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49],
        [-1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68],
        [-1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81],
    ];

    // The two format bits of each level, in the order L, M, Q, H.
    private static readonly int[] FormatBits = [1, 0, 3, 2];

    /// <summary>The dark modules, as [row, column], without a quiet zone.</summary>
    public static bool[,] Encode(string text, StilQrErrorCorrection level)
    {
        ArgumentNullException.ThrowIfNull(text);
        var ecl = (int)level;
        var (mode, bits) = Segment(text);

        var version = 1;
        for (; ; version++)
        {
            if (version > 40)
            {
                throw new ArgumentException("The text is too long for a QR code at this error correction level.", nameof(text));
            }

            if (4 + CountBits(mode, version) + bits.Count <= DataCodewords(version, ecl) * 8)
            {
                break;
            }
        }

        var data = new BitBuffer();
        data.Append(mode, 4);
        data.Append(CharCount(text, mode), CountBits(mode, version));
        data.AddRange(bits);
        var capacity = DataCodewords(version, ecl) * 8;
        data.Append(0, Math.Min(4, capacity - data.Count));
        data.Append(0, (8 - data.Count % 8) % 8);
        for (var pad = 0xEC; data.Count < capacity; pad ^= 0xEC ^ 0x11)
        {
            data.Append(pad, 8);
        }

        var codewords = new byte[data.Count / 8];
        for (var i = 0; i < data.Count; i++)
        {
            codewords[i >> 3] |= (byte)((data[i] ? 1 : 0) << (7 - (i & 7)));
        }

        return new Symbol(version, ecl).Draw(AddErrorCorrection(codewords, version, ecl));
    }

    private static (int Mode, BitBuffer Bits) Segment(string text)
    {
        var bits = new BitBuffer();
        if (text.Length > 0 && text.All(char.IsAsciiDigit))
        {
            for (var i = 0; i < text.Length; i += 3)
            {
                var length = Math.Min(3, text.Length - i);
                bits.Append(int.Parse(text.AsSpan(i, length)), length * 3 + 1);
            }

            return (0x1, bits);
        }

        if (text.All(c => Alphanumeric.Contains(c)))
        {
            for (var i = 0; i < text.Length; i += 2)
            {
                if (i + 1 < text.Length)
                {
                    bits.Append(Alphanumeric.IndexOf(text[i]) * 45 + Alphanumeric.IndexOf(text[i + 1]), 11);
                }
                else
                {
                    bits.Append(Alphanumeric.IndexOf(text[i]), 6);
                }
            }

            return (0x2, bits);
        }

        foreach (var b in Encoding.UTF8.GetBytes(text))
        {
            bits.Append(b, 8);
        }

        return (0x4, bits);
    }

    private static int CharCount(string text, int mode) => mode == 0x4 ? Encoding.UTF8.GetByteCount(text) : text.Length;

    private static int CountBits(int mode, int version)
    {
        var range = version <= 9 ? 0 : version <= 26 ? 1 : 2;
        return mode switch
        {
            0x1 => new[] { 10, 12, 14 }[range],
            0x2 => new[] { 9, 11, 13 }[range],
            _ => new[] { 8, 16, 16 }[range],
        };
    }

    private static int RawDataModules(int version)
    {
        var result = (16 * version + 128) * version + 64;
        if (version >= 2)
        {
            var alignments = version / 7 + 2;
            result -= (25 * alignments - 10) * alignments - 55;
            if (version >= 7)
            {
                result -= 36;
            }
        }

        return result;
    }

    private static int DataCodewords(int version, int ecl) =>
        RawDataModules(version) / 8 - EccCodewordsPerBlock[ecl][version] * ErrorCorrectionBlocks[ecl][version];

    // Splits the data into blocks, adds Reed-Solomon codewords to each and interleaves them.
    private static byte[] AddErrorCorrection(byte[] data, int version, int ecl)
    {
        var blockCount = ErrorCorrectionBlocks[ecl][version];
        var eccLength = EccCodewordsPerBlock[ecl][version];
        var rawCodewords = RawDataModules(version) / 8;
        var shortBlocks = blockCount - rawCodewords % blockCount;
        var shortBlockLength = rawCodewords / blockCount;
        var divisor = ReedSolomonDivisor(eccLength);

        var blocks = new List<byte[]>();
        for (int i = 0, k = 0; i < blockCount; i++)
        {
            var length = shortBlockLength - eccLength + (i < shortBlocks ? 0 : 1);
            var block = data.AsSpan(k, length).ToArray();
            k += length;
            var ecc = ReedSolomonRemainder(block, divisor);
            // Short blocks get a placeholder byte so every block has the same length here.
            blocks.Add([.. block, .. i < shortBlocks ? new byte[1] : [], .. ecc]);
        }

        var result = new List<byte>(rawCodewords);
        for (var i = 0; i < blocks[0].Length; i++)
        {
            for (var j = 0; j < blocks.Count; j++)
            {
                if (i != shortBlockLength - eccLength || j >= shortBlocks)
                {
                    result.Add(blocks[j][i]);
                }
            }
        }

        return [.. result];
    }

    private static byte[] ReedSolomonDivisor(int degree)
    {
        var result = new byte[degree];
        result[degree - 1] = 1;
        var root = 1;
        for (var i = 0; i < degree; i++)
        {
            for (var j = 0; j < degree; j++)
            {
                result[j] = Multiply(result[j], root);
                if (j + 1 < degree)
                {
                    result[j] ^= result[j + 1];
                }
            }

            root = Multiply(root, 0x02);
        }

        return result;
    }

    private static byte[] ReedSolomonRemainder(byte[] data, byte[] divisor)
    {
        var result = new byte[divisor.Length];
        foreach (var b in data)
        {
            var factor = b ^ result[0];
            Array.Copy(result, 1, result, 0, result.Length - 1);
            result[^1] = 0;
            for (var i = 0; i < result.Length; i++)
            {
                result[i] ^= Multiply(divisor[i], factor);
            }
        }

        return result;
    }

    // Multiplication in GF(2^8) modulo x^8 + x^4 + x^3 + x^2 + 1.
    private static byte Multiply(int x, int y)
    {
        var z = 0;
        for (var i = 7; i >= 0; i--)
        {
            z = (z << 1) ^ ((z >> 7) * 0x11D);
            z ^= ((y >> i) & 1) * x;
        }

        return (byte)z;
    }

    private sealed class BitBuffer : List<bool>
    {
        public void Append(int value, int length)
        {
            for (var i = length - 1; i >= 0; i--)
            {
                Add(((value >> i) & 1) != 0);
            }
        }
    }

    // The module grid of one symbol: function patterns, data and the chosen mask.
    private sealed class Symbol(int version, int ecl)
    {
        private readonly int _size = version * 4 + 17;
        private readonly bool[,] _modules = new bool[version * 4 + 17, version * 4 + 17];
        private readonly bool[,] _function = new bool[version * 4 + 17, version * 4 + 17];

        public bool[,] Draw(byte[] codewords)
        {
            DrawFunctionPatterns();
            DrawCodewords(codewords);

            var best = 0;
            var lowest = int.MaxValue;
            for (var mask = 0; mask < 8; mask++)
            {
                ApplyMask(mask);
                DrawFormatBits(mask);
                var penalty = Penalty();
                if (penalty < lowest)
                {
                    best = mask;
                    lowest = penalty;
                }

                ApplyMask(mask);
            }

            ApplyMask(best);
            DrawFormatBits(best);
            return _modules;
        }

        private void Set(int x, int y, bool dark)
        {
            _modules[y, x] = dark;
            _function[y, x] = true;
        }

        private void DrawFunctionPatterns()
        {
            for (var i = 0; i < _size; i++)
            {
                Set(6, i, i % 2 == 0);
                Set(i, 6, i % 2 == 0);
            }

            DrawFinder(3, 3);
            DrawFinder(_size - 4, 3);
            DrawFinder(3, _size - 4);

            var positions = AlignmentPositions();
            var count = positions.Length;
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    // Not where the three finder patterns are.
                    if (!(i == 0 && j == 0 || i == 0 && j == count - 1 || i == count - 1 && j == 0))
                    {
                        DrawAlignment(positions[i], positions[j]);
                    }
                }
            }

            DrawFormatBits(0);
            DrawVersion();
        }

        private void DrawFinder(int x, int y)
        {
            for (var dy = -4; dy <= 4; dy++)
            {
                for (var dx = -4; dx <= 4; dx++)
                {
                    var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    if (x + dx >= 0 && x + dx < _size && y + dy >= 0 && y + dy < _size)
                    {
                        Set(x + dx, y + dy, distance != 2 && distance != 4);
                    }
                }
            }
        }

        private void DrawAlignment(int x, int y)
        {
            for (var dy = -2; dy <= 2; dy++)
            {
                for (var dx = -2; dx <= 2; dx++)
                {
                    Set(x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
                }
            }
        }

        private int[] AlignmentPositions()
        {
            if (version == 1)
            {
                return [];
            }

            var count = version / 7 + 2;
            var step = (version * 8 + count * 3 + 5) / (count * 4 - 4) * 2;
            var result = new int[count];
            result[0] = 6;
            for (int i = count - 1, position = _size - 7; i >= 1; i--, position -= step)
            {
                result[i] = position;
            }

            return result;
        }

        private void DrawFormatBits(int mask)
        {
            var data = FormatBits[ecl] << 3 | mask;
            var remainder = data;
            for (var i = 0; i < 10; i++)
            {
                remainder = (remainder << 1) ^ ((remainder >> 9) * 0x537);
            }

            var bits = (data << 10 | remainder) ^ 0x5412;
            bool Bit(int i) => ((bits >> i) & 1) != 0;

            for (var i = 0; i <= 5; i++)
            {
                Set(8, i, Bit(i));
            }

            Set(8, 7, Bit(6));
            Set(8, 8, Bit(7));
            Set(7, 8, Bit(8));
            for (var i = 9; i < 15; i++)
            {
                Set(14 - i, 8, Bit(i));
            }

            for (var i = 0; i < 8; i++)
            {
                Set(_size - 1 - i, 8, Bit(i));
            }

            for (var i = 8; i < 15; i++)
            {
                Set(8, _size - 15 + i, Bit(i));
            }

            Set(8, _size - 8, true);
        }

        private void DrawVersion()
        {
            if (version < 7)
            {
                return;
            }

            var remainder = version;
            for (var i = 0; i < 12; i++)
            {
                remainder = (remainder << 1) ^ ((remainder >> 11) * 0x1F25);
            }

            var bits = version << 12 | remainder;
            for (var i = 0; i < 18; i++)
            {
                var dark = ((bits >> i) & 1) != 0;
                var a = _size - 11 + i % 3;
                var b = i / 3;
                Set(a, b, dark);
                Set(b, a, dark);
            }
        }

        // Places the codewords in the zigzag order, two columns at a time from the right.
        private void DrawCodewords(byte[] codewords)
        {
            var i = 0;
            for (var right = _size - 1; right >= 1; right -= 2)
            {
                if (right == 6)
                {
                    right = 5;
                }

                for (var vertical = 0; vertical < _size; vertical++)
                {
                    for (var j = 0; j < 2; j++)
                    {
                        var x = right - j;
                        var upward = ((right + 1) & 2) == 0;
                        var y = upward ? _size - 1 - vertical : vertical;
                        if (!_function[y, x] && i < codewords.Length * 8)
                        {
                            _modules[y, x] = ((codewords[i >> 3] >> (7 - (i & 7))) & 1) != 0;
                            i++;
                        }
                    }
                }
            }
        }

        private void ApplyMask(int mask)
        {
            for (var y = 0; y < _size; y++)
            {
                for (var x = 0; x < _size; x++)
                {
                    var invert = mask switch
                    {
                        0 => (x + y) % 2 == 0,
                        1 => y % 2 == 0,
                        2 => x % 3 == 0,
                        3 => (x + y) % 3 == 0,
                        4 => (x / 3 + y / 2) % 2 == 0,
                        5 => x * y % 2 + x * y % 3 == 0,
                        6 => (x * y % 2 + x * y % 3) % 2 == 0,
                        _ => ((x + y) % 2 + x * y % 3) % 2 == 0,
                    };

                    if (invert && !_function[y, x])
                    {
                        _modules[y, x] = !_modules[y, x];
                    }
                }
            }
        }

        // The four penalty rules of the standard; the mask with the lowest score is used.
        private int Penalty()
        {
            var penalty = 0;
            for (var line = 0; line < 2; line++)
            {
                for (var a = 0; a < _size; a++)
                {
                    var run = 0;
                    var previous = false;
                    for (var b = 0; b < _size; b++)
                    {
                        var dark = line == 0 ? _modules[a, b] : _modules[b, a];
                        if (b > 0 && dark == previous)
                        {
                            run++;
                            if (run == 5)
                            {
                                penalty += 3;
                            }
                            else if (run > 5)
                            {
                                penalty++;
                            }
                        }
                        else
                        {
                            run = 1;
                        }

                        previous = dark;

                        // A finder-like pattern: 1:1:3:1:1 with four light modules on one side.
                        if (b >= 10 && IsFinderLike(line, a, b - 10))
                        {
                            penalty += 40;
                        }
                    }
                }
            }

            var darkCount = 0;
            for (var y = 0; y < _size; y++)
            {
                for (var x = 0; x < _size; x++)
                {
                    if (_modules[y, x])
                    {
                        darkCount++;
                    }

                    if (x < _size - 1 && y < _size - 1 && _modules[y, x] == _modules[y, x + 1]
                        && _modules[y, x] == _modules[y + 1, x] && _modules[y, x] == _modules[y + 1, x + 1])
                    {
                        penalty += 3;
                    }
                }
            }

            var total = _size * _size;
            var k = (Math.Abs(darkCount * 20 - total * 10) + total - 1) / total - 1;
            return penalty + k * 10;
        }

        private static readonly bool[] FinderPattern = [true, false, true, true, true, false, true, false, false, false, false];

        private bool IsFinderLike(int line, int a, int start)
        {
            bool At(int i) => line == 0 ? _modules[a, start + i] : _modules[start + i, a];

            var forward = true;
            var backward = true;
            for (var i = 0; i < 11; i++)
            {
                forward &= At(i) == FinderPattern[i];
                backward &= At(i) == FinderPattern[10 - i];
            }

            return forward || backward;
        }
    }
}
