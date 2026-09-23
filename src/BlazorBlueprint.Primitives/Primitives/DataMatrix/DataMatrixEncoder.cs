using System.Text;

namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Encodes text as a classic Data Matrix ECC 200 symbol.</summary>
/// <remarks>
/// Latin-1 text uses the default character set. Text beyond Latin-1 is encoded as UTF-8 with ECI
/// assignment 26. GS1 input must already be an element string; ASCII group separators (U+001D)
/// become FNC1 separators. Parenthesized, human-readable AI notation is not parsed.
/// </remarks>
public static class DataMatrixEncoder
{
    private const byte Fnc1 = 232;
    private const byte Unlatch = 254;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly DataMatrixEncodingMode[] TripletModes =
        [DataMatrixEncodingMode.C40, DataMatrixEncodingMode.Text];

    /// <summary>Encodes a value into a Data Matrix ECC 200 module grid.</summary>
    /// <param name="value">The nonempty text to encode.</param>
    /// <param name="mode">The encodation mode, or Auto to choose compact segments.</param>
    /// <param name="shape">The allowed symbol shape. Rectangle selects only classic rectangles.</param>
    /// <param name="isGs1">
    /// Prefix FNC1 and encode U+001D separators as FNC1. Supply a GS1 element string without AI
    /// parentheses, with U+001D after variable-length elements when another element follows.
    /// </param>
    /// <returns>The finished headless symbol.</returns>
    /// <exception cref="ArgumentException">The value is empty, invalid, or too large.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An enum value is unknown.</exception>
    public static DataMatrixSymbol Encode(
        string value,
        DataMatrixEncodingMode mode = DataMatrixEncodingMode.Auto,
        DataMatrixSymbolShape shape = DataMatrixSymbolShape.Auto,
        bool isGs1 = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        if (!Enum.IsDefined(shape))
        {
            throw new ArgumentOutOfRangeException(nameof(shape));
        }

        var usesUtf8Eci = value.Any(c => c > 255);
        var payload = usesUtf8Eci ? StrictUtf8.GetBytes(value) : value.Select(c => (byte)c).ToArray();
        // Digit pairs are the densest possible encodation, at one codeword for two bytes.
        // This cheap bound avoids a quadratic mode search for clearly oversized user input.
        var maximumDataCapacity = shape == DataMatrixSymbolShape.Rectangle ? 49 : 1558;
        if (payload.Length > maximumDataCapacity * 2)
        {
            throw new ArgumentException("The value is too long for a Data Matrix ECC 200 symbol.", nameof(value));
        }

        var data = EncodeDataCore(payload, mode, isGs1, usesUtf8Eci,
            out var terminalMode, out var modesUsed);
        var withoutUnlatch = terminalMode is not null
            ? DataMatrixSymbolInfo.Choose(data.Count - 1, shape) : null;
        var availableAfterPayload = withoutUnlatch?.DataCapacity - (data.Count - 1);
        var omitUnlatch = availableAfterPayload == 0
            || (terminalMode == DataMatrixEncodingMode.Edifact && availableAfterPayload is >= 1 and <= 2);
        if (omitUnlatch)
        {
            data.RemoveAt(data.Count - 1);
        }

        var info = (omitUnlatch ? withoutUnlatch : DataMatrixSymbolInfo.Choose(data.Count, shape))
            ?? throw new ArgumentException(
                $"The value needs {data.Count} data codewords and does not fit a {shape} Data Matrix ECC 200 symbol.",
                nameof(value));

        Pad(data, info.DataCapacity);
        var allCodewords = DataMatrixErrorCorrection.Add([.. data], info);
        var modules = new DataMatrixPlacement(allCodewords, info).Draw();

        return new DataMatrixSymbol(
            info.Width, info.Height, info.DataCapacity, info.ErrorCodewords,
            info.RegionWidth, info.RegionHeight, mode, modesUsed,
            info.Width == info.Height ? DataMatrixSymbolShape.Square : DataMatrixSymbolShape.Rectangle,
            isGs1, usesUtf8Eci, modules);
    }

    internal static List<byte> EncodeData(
        ReadOnlySpan<byte> payload,
        DataMatrixEncodingMode mode,
        bool isGs1 = false,
        bool usesUtf8Eci = false) =>
        EncodeDataCore(payload, mode, isGs1, usesUtf8Eci, out _, out _);

    private static List<byte> EncodeDataCore(
        ReadOnlySpan<byte> payload,
        DataMatrixEncodingMode mode,
        bool isGs1,
        bool usesUtf8Eci,
        out DataMatrixEncodingMode? terminalMode,
        out IReadOnlyList<DataMatrixEncodingMode> modesUsed)
    {
        var output = new List<byte>(payload.Length + 8);
        var used = new List<DataMatrixEncodingMode>();
        terminalMode = null;
        if (isGs1)
        {
            output.Add(Fnc1);
        }

        if (usesUtf8Eci)
        {
            // ECI assignment 26 is represented by the ECI introducer and assignment + 1.
            output.Add(241);
            output.Add(27);
        }

        if (mode == DataMatrixEncodingMode.Auto)
        {
            terminalMode = EncodeAutomatic(payload, output, isGs1, used);
        }
        else if (mode == DataMatrixEncodingMode.Ascii)
        {
            EncodeAscii(payload, output, isGs1);
            used.Add(DataMatrixEncodingMode.Ascii);
        }
        else if (mode == DataMatrixEncodingMode.Base256)
        {
            // FNC1 separators must remain codeword 232, outside Base 256.
            var start = 0;
            while (start < payload.Length)
            {
                if (isGs1 && payload[start] == 29)
                {
                    output.Add(Fnc1);
                    AddMode(used, DataMatrixEncodingMode.Ascii);
                    start++;
                    continue;
                }

                var end = start;
                while (end < payload.Length && (!isGs1 || payload[end] != 29))
                {
                    end++;
                }

                WriteBase256(payload[start..end], output, end == payload.Length);
                AddMode(used, DataMatrixEncodingMode.Base256);
                start = end;
            }
        }
        else
        {
            terminalMode = EncodeCompaction(payload, mode, output, isGs1, used);
        }

        modesUsed = used.AsReadOnly();
        return output;
    }

    private static DataMatrixEncodingMode? EncodeAutomatic(
        ReadOnlySpan<byte> payload,
        List<byte> output,
        bool isGs1,
        List<DataMatrixEncodingMode> used)
    {
        var count = payload.Length;
        var cost = new int[count + 1];
        var choice = new Route[count];

        for (var i = count - 1; i >= 0; i--)
        {
            if (isGs1 && payload[i] == 29)
            {
                cost[i] = 1 + cost[i + 1];
                choice[i] = new Route(DataMatrixEncodingMode.Ascii, 1);
                continue;
            }

            cost[i] = (payload[i] < 128 ? 1 : 2) + cost[i + 1];
            choice[i] = new Route(DataMatrixEncodingMode.Ascii, 1);
            if (i + 1 < count && IsDigit(payload[i]) && IsDigit(payload[i + 1]))
            {
                Consider(1 + cost[i + 2], DataMatrixEncodingMode.Ascii, 2);
            }

            foreach (var mode in TripletModes)
            {
                var valueCount = 0;
                for (var end = i; end < count && (!isGs1 || payload[end] != 29); end++)
                {
                    valueCount += TripletValueCount(payload[end], mode);
                    if (valueCount % 3 == 0)
                    {
                        Consider(1 + (valueCount / 3 * 2) + (end + 1 == count ? 0 : 1)
                            + cost[end + 1], mode, end - i + 1);
                    }
                }
            }

            var x12Length = 0;
            for (var end = i; end < count && X12Value(payload[end]) >= 0
                && (!isGs1 || payload[end] != 29); end++)
            {
                x12Length++;
                if (x12Length % 3 == 0)
                {
                    Consider(1 + (x12Length / 3 * 2) + (end + 1 == count ? 0 : 1)
                        + cost[end + 1],
                        DataMatrixEncodingMode.X12, x12Length);
                }
            }

            var edifactLength = 0;
            for (var end = i; end < count && IsEdifact(payload[end])
                && (!isGs1 || payload[end] != 29); end++)
            {
                edifactLength++;
                if (edifactLength % 4 == 0)
                {
                    Consider(1 + (edifactLength / 4 * 3) + (end + 1 == count ? 0 : 1)
                        + cost[end + 1],
                        DataMatrixEncodingMode.Edifact, edifactLength);
                }
            }

            for (var length = 1; length <= Math.Min(1556, count - i)
                && (!isGs1 || payload[i + length - 1] != 29); length++)
            {
                if (length == 1556 && i + length != count)
                {
                    continue;
                }

                var lengthField = length <= 249 ? 1 : 2;
                if (length == 1556)
                {
                    lengthField = 1;
                }

                Consider(1 + lengthField + length + cost[i + length],
                    DataMatrixEncodingMode.Base256, length);
            }

            void Consider(int candidate, DataMatrixEncodingMode mode, int length)
            {
                if (candidate < cost[i])
                {
                    cost[i] = candidate;
                    choice[i] = new Route(mode, length);
                }
            }
        }

        DataMatrixEncodingMode? terminalMode = null;
        for (var index = 0; index < count;)
        {
            var route = choice[index];
            var slice = payload.Slice(index, route.Length);
            WriteSegment(slice, route.Mode, output, isGs1, index + route.Length == count);
            AddMode(used, route.Mode);
            index += route.Length;
            terminalMode = route.Mode is DataMatrixEncodingMode.C40 or DataMatrixEncodingMode.Text
                or DataMatrixEncodingMode.X12 or DataMatrixEncodingMode.Edifact ? route.Mode : null;
        }

        return terminalMode;
    }

    private static DataMatrixEncodingMode? EncodeCompaction(
        ReadOnlySpan<byte> payload,
        DataMatrixEncodingMode mode,
        List<byte> output,
        bool isGs1,
        List<DataMatrixEncodingMode> used)
    {
        var index = 0;
        DataMatrixEncodingMode? terminalMode = null;
        while (index < payload.Length)
        {
            if (isGs1 && payload[index] == 29)
            {
                output.Add(Fnc1);
                AddMode(used, DataMatrixEncodingMode.Ascii);
                index++;
                terminalMode = null;
                continue;
            }

            var end = index;
            var valueCount = 0;
            var lastFull = index;
            while (end < payload.Length && (!isGs1 || payload[end] != 29))
            {
                if ((mode == DataMatrixEncodingMode.X12 && X12Value(payload[end]) < 0)
                    || (mode == DataMatrixEncodingMode.Edifact && !IsEdifact(payload[end])))
                {
                    throw new ArgumentException($"{mode} cannot encode character U+{payload[end]:X4}.", nameof(payload));
                }

                valueCount += mode is DataMatrixEncodingMode.C40 or DataMatrixEncodingMode.Text
                    ? TripletValueCount(payload[end], mode) : 1;
                end++;
                if (valueCount % (mode == DataMatrixEncodingMode.Edifact ? 4 : 3) == 0)
                {
                    lastFull = end;
                }
            }

            if (lastFull > index)
            {
                WriteSegment(payload[index..lastFull], mode, output, isGs1, lastFull == payload.Length);
                AddMode(used, mode);
                terminalMode = mode;
            }

            EncodeAscii(payload[lastFull..end], output, isGs1);
            if (lastFull < end)
            {
                AddMode(used, DataMatrixEncodingMode.Ascii);
                terminalMode = null;
            }

            index = end;
        }

        return terminalMode;
    }

    private static void WriteSegment(
        ReadOnlySpan<byte> payload,
        DataMatrixEncodingMode mode,
        List<byte> output,
        bool isGs1,
        bool isFinal)
    {
        switch (mode)
        {
            case DataMatrixEncodingMode.Ascii:
                EncodeAscii(payload, output, isGs1);
                break;
            case DataMatrixEncodingMode.C40 or DataMatrixEncodingMode.Text or DataMatrixEncodingMode.X12:
                WriteTriplets(payload, mode, output);
                break;
            case DataMatrixEncodingMode.Edifact:
                WriteEdifact(payload, output);
                break;
            case DataMatrixEncodingMode.Base256:
                WriteBase256(payload, output, isFinal);
                break;
        }
    }

    private static void EncodeAscii(ReadOnlySpan<byte> payload, List<byte> output, bool isGs1)
    {
        for (var i = 0; i < payload.Length; i++)
        {
            var current = payload[i];
            if (isGs1 && current == 29)
            {
                output.Add(Fnc1);
            }
            else if (i + 1 < payload.Length && IsDigit(current) && IsDigit(payload[i + 1]))
            {
                output.Add((byte)(130 + ((current - '0') * 10) + payload[++i] - '0'));
            }
            else if (current < 128)
            {
                output.Add((byte)(current + 1));
            }
            else
            {
                output.Add(235);
                output.Add((byte)(current - 127));
            }
        }
    }

    private static bool IsDigit(byte value) => value is >= (byte)'0' and <= (byte)'9';

    private static void AddMode(List<DataMatrixEncodingMode> used, DataMatrixEncodingMode mode)
    {
        if (!used.Contains(mode))
        {
            used.Add(mode);
        }
    }

    private static int TripletValueCount(byte value, DataMatrixEncodingMode mode)
    {
        if (value >= 128)
        {
            return 2 + TripletValueCount((byte)(value - 128), mode);
        }

        if (value == ' ' || IsDigit(value)
            || (mode == DataMatrixEncodingMode.C40 && value is >= (byte)'A' and <= (byte)'Z')
            || (mode == DataMatrixEncodingMode.Text && value is >= (byte)'a' and <= (byte)'z'))
        {
            return 1;
        }

        return 2;
    }

    private static void WriteTriplets(ReadOnlySpan<byte> payload, DataMatrixEncodingMode mode, List<byte> output)
    {
        output.Add(mode switch
        {
            DataMatrixEncodingMode.C40 => 230,
            DataMatrixEncodingMode.Text => 239,
            _ => 238,
        });

        var values = new List<int>(payload.Length * 2);
        foreach (var value in payload)
        {
            if (mode == DataMatrixEncodingMode.X12)
            {
                values.Add(X12Value(value));
            }
            else
            {
                AppendTripletValues(value, mode, values);
            }
        }

        for (var i = 0; i < values.Count; i += 3)
        {
            var packed = (1600 * values[i]) + (40 * values[i + 1]) + values[i + 2] + 1;
            output.Add((byte)(packed >>> 8));
            output.Add((byte)packed);
        }

        output.Add(Unlatch);
    }

    private static void AppendTripletValues(byte value, DataMatrixEncodingMode mode, List<int> output)
    {
        if (value >= 128)
        {
            output.Add(1);
            output.Add(30);
            AppendTripletValues((byte)(value - 128), mode, output);
        }
        else if (value == ' ')
        {
            output.Add(3);
        }
        else if (IsDigit(value))
        {
            output.Add(value - '0' + 4);
        }
        else if ((mode == DataMatrixEncodingMode.C40 && value is >= (byte)'A' and <= (byte)'Z')
            || (mode == DataMatrixEncodingMode.Text && value is >= (byte)'a' and <= (byte)'z'))
        {
            output.Add(value - (mode == DataMatrixEncodingMode.C40 ? 'A' : 'a') + 14);
        }
        else if (value < 32)
        {
            output.Add(0);
            output.Add(value);
        }
        else if (value <= 47)
        {
            output.Add(1);
            output.Add(value - 33);
        }
        else if (value <= 64)
        {
            output.Add(1);
            output.Add(value - 58 + 15);
        }
        else if (value <= 95)
        {
            if (mode == DataMatrixEncodingMode.Text && value <= 'Z')
            {
                output.Add(2);
                output.Add(value - 'A' + 1);
            }
            else
            {
                output.Add(1);
                output.Add(value - 91 + 22);
            }
        }
        else
        {
            output.Add(2);
            output.Add(mode == DataMatrixEncodingMode.Text
                ? value == '`' ? 0 : value - 123 + 27
                : value - 96);
        }
    }

    private static int X12Value(byte value) => value switch
    {
        13 => 0,
        (byte)'*' => 1,
        (byte)'>' => 2,
        (byte)' ' => 3,
        >= (byte)'0' and <= (byte)'9' => value - '0' + 4,
        >= (byte)'A' and <= (byte)'Z' => value - 'A' + 14,
        _ => -1,
    };

    private static bool IsEdifact(byte value) => value is >= 32 and <= 94;

    private static void WriteEdifact(ReadOnlySpan<byte> payload, List<byte> output)
    {
        output.Add(240);
        for (var i = 0; i < payload.Length; i += 4)
        {
            var bits = ((payload[i] & 63) << 18)
                | ((payload[i + 1] & 63) << 12)
                | ((payload[i + 2] & 63) << 6)
                | (payload[i + 3] & 63);
            output.Add((byte)(bits >>> 16));
            output.Add((byte)(bits >>> 8));
            output.Add((byte)bits);
        }

        // The six-bit 31 unlatch, padded to a byte, returns the decoder to ASCII.
        output.Add(124);
    }

    private static void WriteBase256(ReadOnlySpan<byte> payload, List<byte> output, bool isFinal)
    {
        if (payload.Length > 1555 && (payload.Length != 1556 || !isFinal))
        {
            throw new ArgumentException("A Base 256 segment cannot exceed 1556 bytes.", nameof(payload));
        }

        output.Add(231);
        if (payload.Length == 1556)
        {
            // Zero means the decoder consumes the rest of the symbol. This is valid only for a
            // final segment which fills the 144x144 symbol exactly.
            AddRandomized(output, 0);
        }
        else if (payload.Length <= 249)
        {
            AddRandomized(output, (byte)payload.Length);
        }
        else
        {
            AddRandomized(output, (byte)((payload.Length / 250) + 249));
            AddRandomized(output, (byte)(payload.Length % 250));
        }

        foreach (var value in payload)
        {
            AddRandomized(output, value);
        }
    }

    private static void AddRandomized(List<byte> output, byte value)
    {
        var position = output.Count + 1;
        output.Add((byte)((value + (149 * position % 255) + 1) % 256));
    }

    internal static void Pad(List<byte> output, int capacity)
    {
        if (output.Count < capacity)
        {
            output.Add(129);
        }

        while (output.Count < capacity)
        {
            var position = output.Count + 1;
            var value = 129 + (149 * position % 253) + 1;
            output.Add((byte)(value <= 254 ? value : value - 254));
        }
    }

    private readonly record struct Route(DataMatrixEncodingMode Mode, int Length);
}
