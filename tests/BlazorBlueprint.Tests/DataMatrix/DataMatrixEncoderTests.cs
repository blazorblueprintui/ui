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
        AssertRefused(DataMatrixFormatError.InvalidText, () => DataMatrixEncoder.Encode("\uD800"));
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
        AssertRefused(DataMatrixFormatError.TooLong, () => DataMatrixEncoder.Encode(new string('1', 99),
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
        AssertRefused(DataMatrixFormatError.TooLong, () => DataMatrixEncoder.Encode(
            new string('ÿ', 1557), DataMatrixEncodingMode.Base256));
    }

    [Fact]
    public void EverySymbolPlacesDataAndDrawsRegionBorders()
    {
        Assert.Equal(30, DataMatrixSymbolInfo.Classic.Length);
        Assert.Equal(18, DataMatrixSymbolInfo.Dmre.Length);
        Assert.Equal(48, DataMatrixSymbolInfo.All.Length);
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
        foreach (var info in DataMatrixSymbolInfo.Classic)
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
    public void DmreTableListsTheEighteenIsoIec21471Sizes()
    {
        (int Width, int Height)[] expected =
        [
            (48, 8), (64, 8), (80, 8), (96, 8), (120, 8), (144, 8), (64, 12), (88, 12), (64, 16),
            (36, 20), (44, 20), (64, 20), (48, 22), (48, 24), (64, 24), (40, 26), (48, 26), (64, 26),
        ];

        Assert.Equal(expected, DataMatrixSymbolInfo.Dmre.Select(info => (info.Width, info.Height)));
        Assert.All(DataMatrixSymbolInfo.Dmre, info => Assert.True(info.IsDmre));
        Assert.DoesNotContain(DataMatrixSymbolInfo.Classic, info => info.IsDmre);
    }

    [Fact]
    public void EveryDmreSizeDecodesAtItsAsciiCapacity()
    {
        // Several DMRE sizes are never the smallest choice for any length (8x80 holds 32
        // codewords, as does the smaller classic 16x36), so place each size directly rather than
        // through symbol selection. ZXing's decoder sizes each symbol from its own DMRE table.
        foreach (var info in DataMatrixSymbolInfo.Dmre)
        {
            var value = new string('7', info.DataCapacity * 2);
            var data = DataMatrixEncoder.EncodeData(Encoding.ASCII.GetBytes(value), DataMatrixEncodingMode.Ascii);
            DataMatrixEncoder.Pad(data, info.DataCapacity);
            var modules = new DataMatrixPlacement(DataMatrixErrorCorrection.Add([.. data], info), info).Draw();

            Assert.True(value == Decode(modules, info.Width, info.Height), $"{info.Width}x{info.Height}");
        }
    }

    [Fact]
    public void OnlyRectangleExtendedChoosesDmre()
    {
        // 18 codewords: classic rectangles need 12x36 (432 modules), DMRE fits 8x48 (384).
        var value = new string('1', 36);

        var classic = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii, DataMatrixSymbolShape.Rectangle);
        var automatic = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii);
        var extended = DataMatrixEncoder.Encode(value, DataMatrixEncodingMode.Ascii,
            DataMatrixSymbolShape.RectangleExtended);

        Assert.Equal((36, 12, DataMatrixSymbolShape.Rectangle), (classic.Width, classic.Height, classic.Shape));
        Assert.Equal(DataMatrixSymbolShape.Square, automatic.Shape);
        Assert.Equal((48, 8, DataMatrixSymbolShape.RectangleExtended), (extended.Width, extended.Height, extended.Shape));
        Assert.Equal(value, Decode(extended));
    }

    [Fact]
    public void RectangleExtendedReachesTheLargestDmreAndRejectsMore()
    {
        var fits = new string('1', 118 * 2);
        var symbol = DataMatrixEncoder.Encode(fits, DataMatrixEncodingMode.Ascii,
            DataMatrixSymbolShape.RectangleExtended);

        Assert.Equal((64, 26), (symbol.Width, symbol.Height));
        Assert.Equal(fits, Decode(symbol));
        var refused = AssertRefused(DataMatrixFormatError.TooLong, () => DataMatrixEncoder.Encode(
            new string('1', 119 * 2), DataMatrixEncodingMode.Ascii, DataMatrixSymbolShape.RectangleExtended));
        Assert.Equal([DataMatrixSymbolShape.RectangleExtended, 118], refused.Arguments);
    }

    [Fact]
    public void Gs1NotationBecomesAnElementStringWithSeparatorsOnlyAfterVariableLengthElements() =>
        Assert.Equal("01095011015300031727010110ABC123\u001D21XYZ",
            Gs1ElementString.FromHumanReadable("(01)09501101530003(17)270101(10)ABC123(21)XYZ"));

    [Theory]
    [InlineData("(21)A(22)B", "21A\u001D22B")]
    [InlineData("(3103)000189(10)L1", "3103000189" + "10L1")]
    [InlineData("(10)AB(C)(21)X", "10AB(C)\u001D21X")]
    [InlineData("(8200)https://example.com", "8200https://example.com")]
    public void Gs1NotationHandlesFourDigitAisAndLiteralParentheses(string notation, string expected) =>
        Assert.Equal(expected, Gs1ElementString.FromHumanReadable(notation));

    [Theory]
    [InlineData("", DataMatrixFormatError.Gs1MissingAi)]
    [InlineData("0109501101530003", DataMatrixFormatError.Gs1MissingAi)]
    [InlineData("(1)ABC", DataMatrixFormatError.Gs1MissingAi)]
    [InlineData("(12345)ABC", DataMatrixFormatError.Gs1MissingAi)]
    [InlineData("(10)", DataMatrixFormatError.Gs1EmptyElement)]
    [InlineData("(10)ABC(21)", DataMatrixFormatError.Gs1EmptyElement)]
    [InlineData("(01)0950110153000", DataMatrixFormatError.Gs1WrongLength)]
    [InlineData("(17)2701011", DataMatrixFormatError.Gs1WrongLength)]
    [InlineData("(10)A\u001DB", DataMatrixFormatError.Gs1SeparatorInNotation)]
    public void Gs1NotationRejectsMalformedInputWithItsReason(string notation, DataMatrixFormatError reason) =>
        AssertRefused(reason, () => Gs1ElementString.FromHumanReadable(notation));

    [Fact]
    public void Gs1WrongLengthCarriesTheElementAndBothLengths()
    {
        var refused = AssertRefused(DataMatrixFormatError.Gs1WrongLength,
            () => DataMatrixEncoder.Encode("(01)0950110153000(10)ABC", isGs1: true));

        Assert.Equal(["01", 14, 13], refused.Arguments);
        Assert.Equal("GS1 element (01) needs 14 data characters, and this value has 13.", refused.Message);
        Assert.Equal("text", refused.ParamName);
    }

    [Fact]
    public void ExplicitModeRefusalNamesTheModeAndCharacter()
    {
        var lower = AssertRefused(DataMatrixFormatError.CharacterNotInMode,
            () => DataMatrixEncoder.Encode("ABCd", DataMatrixEncodingMode.X12));
        var control = AssertRefused(DataMatrixFormatError.CharacterNotInMode,
            () => DataMatrixEncoder.Encode("AB\tC", DataMatrixEncodingMode.Edifact));

        Assert.Equal([DataMatrixEncodingMode.X12, "d"], lower.Arguments);
        Assert.Equal([DataMatrixEncodingMode.Edifact, "U+0009"], control.Arguments);
        // The GS1 separator is not refused: it becomes FNC1 outside the mode.
        Assert.Equal("\u001DABC\u001DDEF",
            Decode(DataMatrixEncoder.Encode("ABC\u001DDEF", DataMatrixEncodingMode.X12, isGs1: true)));
    }

    [Fact]
    public void Gs1NotationEncodesTheSameSymbolAsTheElementString()
    {
        const string notation = "(01)09501101530003(17)270101(10)ABC123(21)XYZ";
        const string elementString = "01095011015300031727010110ABC123\u001D21XYZ";

        var fromNotation = DataMatrixEncoder.Encode(notation, isGs1: true);
        var fromElementString = DataMatrixEncoder.Encode(elementString, isGs1: true);

        Assert.Equal(Flatten(fromElementString), Flatten(fromNotation));
        Assert.Equal("\u001D" + elementString, Decode(fromNotation));
        // Without IsGs1 the parentheses are ordinary text.
        Assert.Equal(notation, Decode(DataMatrixEncoder.Encode(notation)));
    }

    [Fact]
    public void SymbolSizeNamesMatchTheSymbolTableRowsByColumns()
    {
        var sizes = Enum.GetValues<DataMatrixSymbolSize>().Where(size => size != DataMatrixSymbolSize.Auto).ToArray();
        Assert.Equal(DataMatrixSymbolInfo.All.Length, sizes.Length);

        foreach (var size in sizes)
        {
            var name = size.ToString();
            var digits = name.TrimStart("SquareRectangleDmre".ToCharArray()).Split('x').Select(int.Parse).ToArray();
            var (rows, columns) = digits.Length == 1 ? (digits[0], digits[0]) : (digits[0], digits[1]);
            var info = DataMatrixSymbolInfo.ForSize(size);

            Assert.True((rows, columns) == (info.Height, info.Width), $"{name} is {info.Height}x{info.Width}");
            Assert.Equal(name.StartsWith("Dmre", StringComparison.Ordinal), info.IsDmre);
        }
    }

    [Fact]
    public void FixedSizeKeepsTheSameSymbolWhateverTheLength()
    {
        var shortValue = DataMatrixEncoder.Encode("SN-1", size: DataMatrixSymbolSize.Square16);
        var longValue = DataMatrixEncoder.Encode("SN-2026-000417", size: DataMatrixSymbolSize.Square16);

        Assert.Equal((16, 16), (shortValue.Width, shortValue.Height));
        Assert.Equal((16, 16), (longValue.Width, longValue.Height));
        Assert.Equal("SN-1", Decode(shortValue));
        Assert.Equal("SN-2026-000417", Decode(longValue));
    }

    [Fact]
    public void FixedSizeReachesSizesAutoNeverPicksAndOverridesShape()
    {
        // 8x80 is never the smallest choice: the classic 16x36 holds as much in fewer modules.
        var strip = DataMatrixEncoder.Encode("LOT-42", size: DataMatrixSymbolSize.Dmre8x80);
        var rectangle = DataMatrixEncoder.Encode("A1", shape: DataMatrixSymbolShape.Square,
            size: DataMatrixSymbolSize.Rectangle8x18);

        Assert.Equal((80, 8, DataMatrixSymbolShape.RectangleExtended), (strip.Width, strip.Height, strip.Shape));
        Assert.Equal("LOT-42", Decode(strip));
        Assert.Equal((18, 8, DataMatrixSymbolShape.Rectangle), (rectangle.Width, rectangle.Height, rectangle.Shape));
        Assert.Equal("A1", Decode(rectangle));
    }

    [Fact]
    public void FixedSizeRefusesAValueThatDoesNotFit()
    {
        var refused = AssertRefused(DataMatrixFormatError.DoesNotFitSize,
            () => DataMatrixEncoder.Encode("ABCDEFG", size: DataMatrixSymbolSize.Square10));

        Assert.Equal([DataMatrixSymbolSize.Square10, 3], refused.Arguments);
        Assert.Equal("This value does not fit the Square10 symbol, which holds 3 data codewords.", refused.Message);
        Assert.Throws<ArgumentOutOfRangeException>(() => DataMatrixEncoder.Encode("x", size: (DataMatrixSymbolSize)99));
    }

    [Theory]
    [InlineData("0950110153000", '3')]
    [InlineData("10614141123456789", '7')]
    [InlineData("541234500001", '3')]
    public void Gs1CheckDigitMatchesPublishedExamples(string digits, char expected) =>
        Assert.Equal(expected, Gs1ElementString.CheckDigit(digits));

    [Theory]
    [InlineData("(00)106141411234567897(21)A")]
    [InlineData("(02)09501101530003(37)10")]
    [InlineData("(414)5412345000013(21)A")]
    public void Gs1NotationAcceptsValidCheckDigits(string notation) =>
        Assert.StartsWith(notation[1..3], Gs1ElementString.FromHumanReadable(notation), StringComparison.Ordinal);

    [Fact]
    public void Gs1NotationRefusesAWrongCheckDigitOrNonDigits()
    {
        var wrong = AssertRefused(DataMatrixFormatError.Gs1WrongCheckDigit,
            () => Gs1ElementString.FromHumanReadable("(01)09501101530004(10)ABC"));
        var letters = AssertRefused(DataMatrixFormatError.Gs1NotDigits,
            () => Gs1ElementString.FromHumanReadable("(414)541234500001X"));

        Assert.Equal(["01", "3", "4"], wrong.Arguments);
        Assert.Equal("GS1 element (01) ends in the wrong check digit: it should be 3, not 4.", wrong.Message);
        Assert.Equal(["414"], letters.Arguments);
        // AIs without a check digit, such as the lot (10), are not checked.
        Assert.Equal("10ABC", Gs1ElementString.FromHumanReadable("(10)ABC"));
    }

    [Theory]
    [InlineData("(10)LOT 7", " ")]
    [InlineData("(10)LOTó", "ó")]
    [InlineData("(21)A#1", "#")]
    [InlineData("(21)A\tB", "U+0009")]
    public void Gs1NotationRefusesCharactersOutsideSet82(string notation, string character)
    {
        var refused = AssertRefused(DataMatrixFormatError.Gs1InvalidCharacter,
            () => Gs1ElementString.FromHumanReadable(notation));

        Assert.Equal(notation[1..3], refused.Arguments[0]);
        Assert.Equal(character, refused.Arguments[1]);
    }

    [Fact]
    public void Gs1NotationAcceptsEverySet82Character() =>
        Assert.Equal("21!\"%&'()*+,-./:;<=>?_azAZ09",
            Gs1ElementString.FromHumanReadable("(21)!\"%&'()*+,-./:;<=>?_azAZ09"));

    [Theory]
    [InlineData("(3103)00018X")]
    [InlineData("(17)27O101")]
    [InlineData("(20)A1")]
    public void Gs1NotationRefusesNonDigitsInEveryPredefinedLengthElement(string notation) =>
        AssertRefused(DataMatrixFormatError.Gs1NotDigits, () => Gs1ElementString.FromHumanReadable(notation));

    [Theory]
    [InlineData("(17)271332")]
    [InlineData("(17)270230")]
    [InlineData("(11)250229")]
    [InlineData("(15)270015")]
    [InlineData("(13)270431")]
    public void Gs1NotationRefusesDatesThatDoNotExist(string notation)
    {
        var refused = AssertRefused(DataMatrixFormatError.Gs1InvalidDate,
            () => Gs1ElementString.FromHumanReadable(notation));

        Assert.Equal(notation[4..], refused.Arguments[1]);
    }

    [Theory]
    [InlineData("(17)270101")]
    [InlineData("(17)271200")]
    [InlineData("(11)240229")]
    [InlineData("(16)271231")]
    [InlineData("(12)270430")]
    public void Gs1NotationAcceptsRealDatesAndDayZero(string notation) =>
        Assert.Equal(notation[1..3] + notation[4..], Gs1ElementString.FromHumanReadable(notation));

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
        AssertRefused(DataMatrixFormatError.Empty, () => DataMatrixEncoder.Encode(string.Empty));
        Assert.Throws<ArgumentNullException>(() => DataMatrixEncoder.Encode(null!));
        Assert.Throws<ArgumentNullException>(() => Gs1ElementString.FromHumanReadable(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => DataMatrixEncoder.Encode("x", (DataMatrixEncodingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => DataMatrixEncoder.Encode("x", shape: (DataMatrixSymbolShape)99));
        AssertRefused(DataMatrixFormatError.CharacterNotInMode, () => DataMatrixEncoder.Encode("a", DataMatrixEncodingMode.X12));
        AssertRefused(DataMatrixFormatError.CharacterNotInMode, () => DataMatrixEncoder.Encode("_", DataMatrixEncodingMode.Edifact));
        AssertRefused(DataMatrixFormatError.TooLong, () => DataMatrixEncoder.Encode(new string('1', 4000)));
    }

    [Fact]
    public void RefusalMessageIsTheSentenceAlone()
    {
        // ArgumentException.Message would append a parameter-name suffix from a framework resource
        // that WebAssembly trims; the format exception keeps the sentence on its own.
        var refused = AssertRefused(DataMatrixFormatError.TooLong,
            () => DataMatrixEncoder.Encode(new string('1', 200), DataMatrixEncodingMode.Ascii, DataMatrixSymbolShape.Rectangle));

        Assert.Equal("This value is too long for a Data Matrix symbol of shape Rectangle, which holds at most 49 data codewords.",
            refused.Message);
        Assert.Equal("value", refused.ParamName);
    }

    private static DataMatrixFormatException AssertRefused(DataMatrixFormatError reason, Action action)
    {
        var refused = Assert.Throws<DataMatrixFormatException>(action);
        Assert.Equal(reason, refused.Error);
        return refused;
    }

    private static DataMatrixFormatException AssertRefused(DataMatrixFormatError reason, Func<object> action) =>
        AssertRefused(reason, () => { action(); });

    private static string Decode(DataMatrixSymbol symbol)
    {
        var modules = new bool[symbol.Width * symbol.Height];
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                modules[(y * symbol.Width) + x] = symbol[x, y];
            }
        }

        return Decode(modules, symbol.Width, symbol.Height);
    }

    private static string Decode(bool[] modules, int width, int height)
    {
        var bits = new BitMatrix(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (modules[(y * width) + x])
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
