namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Reed-Solomon ECC 200 over GF(256), using x^8+x^5+x^3+x^2+1.</summary>
internal static class DataMatrixErrorCorrection
{
    private const int FieldPolynomial = 0x12D;

    internal static byte[] Add(byte[] data, DataMatrixSymbolInfo info)
    {
        var result = new byte[info.DataCapacity + info.ErrorCodewords];
        data.CopyTo(result, 0);

        var divisor = Divisor(info.ErrorPerBlock);
        for (var block = 0; block < info.BlockCount; block++)
        {
            var length = info.DataLengthForBlock(block);
            var blockData = new byte[length];
            for (var i = 0; i < length; i++)
            {
                blockData[i] = data[block + (i * info.BlockCount)];
            }

            var remainder = Remainder(blockData, divisor);
            for (var i = 0; i < remainder.Length; i++)
            {
                // In 144x144, ECC interleaving starts with data block 8, then 9, then 0..7.
                // The shorter blocks therefore occupy the first two ECC positions.
                var errorColumn = info.DataCapacity == 1558
                    ? (block + 2) % info.BlockCount : block;
                result[info.DataCapacity + errorColumn + (i * info.BlockCount)] = remainder[i];
            }
        }

        return result;
    }

    internal static byte[] Divisor(int degree)
    {
        var coefficients = new byte[] { 1 };
        byte root = 1;
        for (var i = 1; i <= degree; i++)
        {
            root = Multiply(root, 2);
            var next = new byte[coefficients.Length + 1];
            for (var j = 0; j < coefficients.Length; j++)
            {
                next[j] ^= coefficients[j];
                next[j + 1] ^= Multiply(coefficients[j], root);
            }

            coefficients = next;
        }

        return coefficients[1..];
    }

    internal static byte[] Remainder(ReadOnlySpan<byte> data, byte[] divisor)
    {
        var remainder = new byte[divisor.Length];
        foreach (var codeword in data)
        {
            var factor = (byte)(codeword ^ remainder[0]);
            Array.Copy(remainder, 1, remainder, 0, remainder.Length - 1);
            remainder[^1] = 0;
            for (var i = 0; i < remainder.Length; i++)
            {
                remainder[i] ^= Multiply(factor, divisor[i]);
            }
        }

        return remainder;
    }

    internal static byte Multiply(byte a, byte b)
    {
        var result = 0;
        for (var bit = 7; bit >= 0; bit--)
        {
            result = (result << 1) ^ ((result >>> 7) * FieldPolynomial);
            result ^= ((b >>> bit) & 1) * a;
        }

        return (byte)result;
    }
}
