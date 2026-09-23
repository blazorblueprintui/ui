using System.Text;
using BlazorBlueprint.Primitives.DataMatrix;
using Xunit;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.Datamatrix.Encoder;
using ZXing.Datamatrix.Internal;

namespace BlazorBlueprint.Tests.DataMatrix;

/// <summary>
/// Exact encodation checks and independent encode/decode checks against ZXing.Net. ZXing.Net is
/// referenced by tests only; the runtime encoder has no barcode dependency.
/// </summary>
public class DataMatrixEncoderTests
{
    [Fact]
    public void AsciiCompactsDigitPairsAndUpperShiftsLatin1()
    {
        Assert.Equal([142, 164, 186],
            DataMatrixEncoder.EncodeData("123456"u8, DataMatrixEncodingMode.Ascii));
        Assert.Equal([142, 164, 186, 235, 36],
            DataMatrixEncoder.EncodeData([.. "123456"u8, 0xA3], DataMatrixEncodingMode.Ascii));
    }

    [Fact]
    public void C40MatchesTheIsoSpecExampleInZxingsReferenceTests() =>
        Assert.Equal(
            [230, 88, 88, 40, 8, 107, 147, 59, 67, 126, 206, 78, 126, 144, 121, 35, 47, 254],
            DataMatrixEncoder.EncodeData("A1B2C3D4E5F6G7H8I9J0K1L2"u8, DataMatrixEncodingMode.C40));

    [Fact]
    public void TextTripletsMatchTheReferenceVector() =>
        Assert.Equal([239, 91, 11, 91, 11, 91, 11, 254],
            DataMatrixEncoder.EncodeData("aimaimaim"u8, DataMatrixEncodingMode.Text));

    [Fact]
    public void X12PacksThreeCharactersIntoTwoCodewords() =>
        Assert.Equal([238, 89, 233, 254],
            DataMatrixEncoder.EncodeData("ABC"u8, DataMatrixEncodingMode.X12));

    [Fact]
    public void EdifactPacksFourCharactersAndWritesUnlatch() =>
        Assert.Equal([240, 184, 27, 131, 124],
            DataMatrixEncoder.EncodeData(".A.C"u8, DataMatrixEncodingMode.Edifact));

    [Theory]
    [InlineData(".A.C")]
    [InlineData(".A.C1.3.DATA")]
    public void EdifactOmitsTerminalUnlatchWhenAtMostTwoSlotsRemain(string value) =>
        Assert.Equal(value, Decode(DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Edifact)));

    [Fact]
    public void Base256RandomizesLengthAndDataAtAbsolutePositions() =>
        Assert.Equal([231, 47, 34, 185, 79],
            DataMatrixEncoder.EncodeData("abc"u8, DataMatrixEncodingMode.Base256));

    [Fact]
    public void Base256UsesTwoLengthCodewordsAbove249Bytes()
    {
        var encoded = DataMatrixEncoder.EncodeData(
            Enumerable.Repeat((byte)0xFF, 250).ToArray(), DataMatrixEncodingMode.Base256);

        Assert.Equal(253, encoded.Count);
        Assert.Equal((byte)231, encoded[0]);
        Assert.Equal((byte)38, encoded[1]);
        Assert.Equal((byte)193, encoded[2]);
    }

    [Fact]
    public void PadsWith129ThenRandomized253State()
    {
        var data = DataMatrixEncoder.EncodeData("A"u8, DataMatrixEncodingMode.Ascii);
        DataMatrixEncoder.Pad(data, 5);

        Assert.Equal([66, 129, 70, 220, 115], data);
    }

    [Fact]
    public void UsesDataMatrixFieldAndKnownDegreeFiveGenerator() =>
        Assert.Equal([62, 111, 15, 48, 228], DataMatrixErrorCorrection.Divisor(5));

    [Fact]
    public void Gs1PrefixesAndSeparatesWithFnc1()
    {
        var data = DataMatrixEncoder.EncodeData(
            "011234567890123110LOT\u001D21SERIAL"u8, DataMatrixEncodingMode.Ascii, isGs1: true);

        Assert.Equal((byte)232, data[0]);
        Assert.Equal(2, data.Count(c => c == 232));
        Assert.DoesNotContain((byte)30, data);
    }

    [Fact]
    public void Gs1SymbolRoundTripsThroughIndependentDecoder()
    {
        const string elementString = "01095011015300031727010110ABC123\u001D21XYZ";
        var symbol = DataMatrixEncoder.Encode(elementString, isGs1: true);

        Assert.True(symbol.IsGs1);
        // ZXing exposes the leading FNC1 as U+001D in its raw decoded text.
        Assert.Equal("\u001D" + elementString, Decode(symbol));
    }

    [Fact]
    public void UnicodeUsesUtf8EciAndLatin1DoesNot()
    {
        var unicode = DataMatrixEncoder.Encode("你好", DataMatrixEncodingMode.Base256);
        var latin1 = DataMatrixEncoder.Encode("café", DataMatrixEncodingMode.Base256);

        Assert.True(unicode.UsesUtf8Eci);
        Assert.False(latin1.UsesUtf8Eci);
        Assert.Equal([241, 27], DataMatrixEncoder.EncodeData([], DataMatrixEncodingMode.Ascii,
            usesUtf8Eci: true));
        Assert.Throws<EncoderFallbackException>(() => DataMatrixEncoder.Encode("\uD800"));
    }

    [Theory]
    [InlineData(DataMatrixEncodingMode.Ascii, "1234567890")]
    [InlineData(DataMatrixEncodingMode.C40, "A1B2C3D4E5F6")]
    [InlineData(DataMatrixEncodingMode.Text, "aimaimaim")]
    [InlineData(DataMatrixEncodingMode.X12, "ABC>ABC123>ABCD")]
    [InlineData(DataMatrixEncodingMode.Edifact, ".A.C1.3.DATA.123")]
    [InlineData(DataMatrixEncodingMode.Base256, "café ÿ")]
    [InlineData(DataMatrixEncodingMode.Auto, "AIMAIMAIM1234567890abcdef")]
    [InlineData(DataMatrixEncodingMode.Auto, "你好世界")]
    public void IndependentDecoderReadsEveryMode(DataMatrixEncodingMode mode, string value)
    {
        var symbol = DataMatrixEncoder.Encode(value, mode);
        Assert.Equal(value, Decode(symbol));
    }

    [Theory]
    [InlineData("A", DataMatrixSymbolShape.Square)]
    [InlineData("123456", DataMatrixSymbolShape.Square)]
    [InlineData("123456789012345678901234567890123", DataMatrixSymbolShape.Rectangle)]
    public void MatchesIndependentZxingMatrixForAsciiPayloads(string value, DataMatrixSymbolShape shape)
    {
        var symbol = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii, shape);
        var hint = shape == DataMatrixSymbolShape.Square
            ? SymbolShapeHint.FORCE_SQUARE : SymbolShapeHint.FORCE_RECTANGLE;
        var reference = new DataMatrixWriter().encode(value, BarcodeFormat.DATA_MATRIX, 0, 0,
            new Dictionary<EncodeHintType, object> { [EncodeHintType.DATA_MATRIX_SHAPE] = hint });

        Assert.Equal(reference.Width, symbol.Width);
        Assert.Equal(reference.Height, symbol.Height);
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                Assert.Equal(reference[x, y], symbol[x, y]);
            }
        }
    }

    [Theory]
    [InlineData(204)]
    [InlineData(816)]
    public void MatchesIndependentMatrixAcrossInterleavedBlockSizes(int dataCapacity)
    {
        var value = new string('1', dataCapacity * 2);
        var symbol = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii,
            DataMatrixSymbolShape.Square);
        var reference = new DataMatrixWriter().encode(value, BarcodeFormat.DATA_MATRIX, 0, 0,
            new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.DATA_MATRIX_SHAPE] = SymbolShapeHint.FORCE_SQUARE,
            });

        Assert.Equal(dataCapacity, symbol.DataCapacity);
        Assert.Equal(reference.Width, symbol.Width);
        Assert.Equal(reference.Height, symbol.Height);
        Assert.Equal(value, new ZXing.Datamatrix.Internal.Decoder().decode(reference)?.Text);
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                Assert.Equal(reference[x, y], symbol[x, y]);
            }
        }
    }

    [Fact]
    public void SelectsClassicRectangularBoundaryAndRejectsOversize()
    {
        var symbol = DataMatrixEncoder.Encode(new string('1', 33),
            DataMatrixEncodingMode.Ascii, DataMatrixSymbolShape.Rectangle);

        Assert.Equal(36, symbol.Width);
        Assert.Equal(12, symbol.Height);
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode(new string('1', 99),
            DataMatrixEncodingMode.Ascii, DataMatrixSymbolShape.Rectangle));
    }

    [Fact]
    public void OmitsTerminalUnlatchWhenCompactionFillsTheSymbol()
    {
        var symbol = DataMatrixEncoder.Encode("ABCDEF", DataMatrixEncodingMode.C40,
            DataMatrixSymbolShape.Square);

        Assert.Equal(12, symbol.Width);
        Assert.Equal(5, symbol.DataCapacity);
        Assert.Equal("ABCDEF", Decode(symbol));
    }

    [Fact]
    public void AutomaticModeDoesNotWasteAWholeSymbolForLongRegularRuns()
    {
        var upper = new string('A', 100);
        var lower = new string('a', 100);

        Assert.Equal(DataMatrixEncoder.Encode(upper, DataMatrixEncodingMode.C40).Width,
            DataMatrixEncoder.Encode(upper).Width);
        Assert.Equal(DataMatrixEncoder.Encode(lower, DataMatrixEncodingMode.Text).Width,
            DataMatrixEncoder.Encode(lower).Width);
        Assert.Contains(DataMatrixEncodingMode.C40, DataMatrixEncoder.Encode(upper).ModesUsed);
        Assert.Contains(DataMatrixEncodingMode.Text, DataMatrixEncoder.Encode(lower).ModesUsed);
    }

    [Fact]
    public void AutoIsNoLargerThanAnExplicitModeForRepresentativePayloads()
    {
        foreach (var (value, mode) in new[]
        {
            ("AIMAIMAIMAIMAIM1234567890", DataMatrixEncodingMode.C40),
            ("aimaimaimaimaim1234567890", DataMatrixEncodingMode.Text),
            ("ABC>ABC123>ABC123", DataMatrixEncodingMode.X12),
            (".A.C1.3.DATA.123DATA", DataMatrixEncodingMode.Edifact),
            ("ÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿ", DataMatrixEncodingMode.Base256),
        })
        {
            var automatic = DataMatrixEncoder.Encode(value);
            var explicitSymbol = DataMatrixEncoder.Encode(value, mode);
            Assert.True(automatic.Width * automatic.Height
                <= explicitSymbol.Width * explicitSymbol.Height, value);
        }
    }

    [Fact]
    public void LargestSymbolUsesUnequalDataBlocksAndDecodes()
    {
        // ZXing.Net's 144x144 writer does not decode through its own decoder: it writes ECC in
        // block order 0..9, while the 144x144 reader expects 8,9,0..7. Check the required block
        // lengths here and round-trip our output through its independent decoder instead.
        var info = Assert.Single(DataMatrixSymbolInfo.All, entry => entry.DataCapacity == 1558);
        Assert.Equal(10, info.BlockCount);
        Assert.Equal(156, info.DataLengthForBlock(7));
        Assert.Equal(155, info.DataLengthForBlock(8));
        Assert.Equal(1558, Enumerable.Range(0, 10).Sum(info.DataLengthForBlock));

        var value = new string('ÿ', 1556);
        var symbol = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Base256);
        Assert.Equal(144, symbol.Width);
        Assert.Equal(144, symbol.Height);
        Assert.Equal(value, Decode(symbol));
        Assert.Equal((byte)44, DataMatrixEncoder.EncodeData(
            Enumerable.Repeat((byte)0xFF, 1556).ToArray(), DataMatrixEncodingMode.Base256)[1]);
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode(
            new string('ÿ', 1557), DataMatrixEncodingMode.Base256));
    }

    [Fact]
    public void EveryClassicSymbolPlacesDataAndDrawsRegionBorders()
    {
        Assert.Equal(30, DataMatrixSymbolInfo.All.Length);
        foreach (var info in DataMatrixSymbolInfo.All)
        {
            var data = new byte[info.DataCapacity];
            Array.Fill(data, (byte)129);
            var codewords = DataMatrixErrorCorrection.Add(data, info);
            var modules = new DataMatrixPlacement(codewords, info).Draw();
            Assert.Equal(info.Width * info.Height, modules.Length);
            for (var regionRow = 0; regionRow < info.RegionRows; regionRow++)
            {
                for (var regionColumn = 0; regionColumn < info.RegionColumns; regionColumn++)
                {
                    var left = regionColumn * (info.RegionWidth + 2);
                    var top = regionRow * (info.RegionHeight + 2);
                    for (var x = 0; x < info.RegionWidth + 2; x++)
                    {
                        Assert.Equal(x % 2 == 0, modules[(top * info.Width) + left + x]);
                        Assert.True(modules[((top + info.RegionHeight + 1) * info.Width) + left + x]);
                    }
                }
            }
        }
    }

    [Fact]
    public void EveryClassicSizeDecodesAtItsAsciiCapacity()
    {
        foreach (var info in DataMatrixSymbolInfo.All)
        {
            var value = new string('1', info.DataCapacity * 2);
            var shape = info.Width == info.Height
                ? DataMatrixSymbolShape.Square : DataMatrixSymbolShape.Rectangle;
            var symbol = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii, shape);

            Assert.Equal(info.Width, symbol.Width);
            Assert.Equal(info.Height, symbol.Height);
            Assert.Equal(value, Decode(symbol));
        }
    }

    [Fact]
    public void SameValueAlwaysProducesTheSameModules()
    {
        var first = DataMatrixEncoder.Encode("AIMAIMAIM1234567890abcdef");
        var second = DataMatrixEncoder.Encode("AIMAIMAIM1234567890abcdef");
        Assert.Equal(Flatten(first), Flatten(second));
        Assert.False(first[-1, 0]);
        Assert.False(first[first.Width, 0]);
    }

    [Fact]
    public void AutomaticSegmentsRoundTripForVariedDeterministicPayloads()
    {
        const string alphabet = "ABCDabcd0123456789*>. _-!éÿ";
        var random = new Random(20260923);
        for (var sample = 0; sample < 100; sample++)
        {
            var length = random.Next(1, 121);
            var value = new string(Enumerable.Range(0, length)
                .Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());

            var symbol = DataMatrixEncoder.Encode(value);
            var decoded = Decode(symbol);
            Assert.True(value == decoded,
                $"Sample {sample}, {symbol.Width}x{symbol.Height}, modes {string.Join(',', symbol.ModesUsed)}: expected [{value}], decoded [{decoded}]");
        }
    }

    [Fact]
    public void RejectsInvalidArgumentsAndUnsupportedModeCharacters()
    {
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode(string.Empty));
        Assert.Throws<ArgumentNullException>(() => DataMatrixEncoder.Encode(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => DataMatrixEncoder.Encode("x", (DataMatrixEncodingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => DataMatrixEncoder.Encode("x", shape: (DataMatrixSymbolShape)99));
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode("a", DataMatrixEncodingMode.X12));
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode("_", DataMatrixEncodingMode.Edifact));
        Assert.Throws<ArgumentException>(() => DataMatrixEncoder.Encode(new string('1', 4000)));
    }

    private static string Decode(DataMatrixSymbol symbol)
    {
        var bits = new BitMatrix(symbol.Width, symbol.Height);
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                if (symbol[x, y])
                {
                    bits[x, y] = true;
                }
            }
        }

        return new ZXing.Datamatrix.Internal.Decoder().decode(bits)?.Text
            ?? throw new Xunit.Sdk.XunitException("ZXing could not decode the symbol.");
    }

    private static string Flatten(DataMatrixSymbol symbol)
    {
        var builder = new StringBuilder(symbol.Width * symbol.Height);
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                builder.Append(symbol[x, y] ? '1' : '0');
            }
        }

        return builder.ToString();
    }
}
