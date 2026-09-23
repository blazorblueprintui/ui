namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>ECC 200 Utah and corner placement, followed by region borders.</summary>
internal sealed class DataMatrixPlacement(byte[] codewords, DataMatrixSymbolInfo info)
{
    private readonly int columns = info.RegionWidth * info.RegionColumns;
    private readonly int rows = info.RegionHeight * info.RegionRows;
    private readonly sbyte[] cells = Enumerable.Repeat((sbyte)-1,
        info.RegionWidth * info.RegionColumns * info.RegionHeight * info.RegionRows).ToArray();

    internal bool[] Draw()
    {
        var position = 0;
        var row = 4;
        var column = 0;

        do
        {
            if (row == rows && column == 0)
            {
                Corner1(position++);
            }

            if (row == rows - 2 && column == 0 && columns % 4 != 0)
            {
                Corner2(position++);
            }

            if (row == rows - 2 && column == 0 && columns % 8 == 4)
            {
                Corner3(position++);
            }

            if (row == rows + 4 && column == 2 && columns % 8 == 0)
            {
                Corner4(position++);
            }

            do
            {
                if (row < rows && column >= 0 && IsUnset(row, column))
                {
                    Utah(row, column, position++);
                }

                row -= 2;
                column += 2;
            }
            while (row >= 0 && column < columns);

            row++;
            column += 3;

            do
            {
                if (row >= 0 && column < columns && IsUnset(row, column))
                {
                    Utah(row, column, position++);
                }

                row += 2;
                column -= 2;
            }
            while (row < rows && column >= 0);

            row += 3;
            column++;
        }
        while (row < rows || column < columns);

        if (IsUnset(rows - 1, columns - 1))
        {
            cells[((rows - 1) * columns) + columns - 1] = 1;
            cells[((rows - 2) * columns) + columns - 2] = 1;
        }

        var modules = new bool[info.Width * info.Height];
        for (var regionRow = 0; regionRow < info.RegionRows; regionRow++)
        {
            for (var regionColumn = 0; regionColumn < info.RegionColumns; regionColumn++)
            {
                var left = regionColumn * (info.RegionWidth + 2);
                var top = regionRow * (info.RegionHeight + 2);
                for (var x = 0; x < info.RegionWidth + 2; x++)
                {
                    modules[(top * info.Width) + left + x] = x % 2 == 0;
                    modules[((top + info.RegionHeight + 1) * info.Width) + left + x] = true;
                }

                for (var y = 0; y < info.RegionHeight; y++)
                {
                    var outputRow = top + y + 1;
                    modules[(outputRow * info.Width) + left] = true;
                    for (var x = 0; x < info.RegionWidth; x++)
                    {
                        var dataRow = (regionRow * info.RegionHeight) + y;
                        var dataColumn = (regionColumn * info.RegionWidth) + x;
                        modules[(outputRow * info.Width) + left + x + 1] =
                            cells[(dataRow * columns) + dataColumn] == 1;
                    }

                    modules[(outputRow * info.Width) + left + info.RegionWidth + 1] = y % 2 == 0;
                }
            }
        }

        return modules;
    }

    private bool IsUnset(int row, int column) => cells[(row * columns) + column] < 0;

    private void Module(int row, int column, int position, int bit)
    {
        if (row < 0)
        {
            row += rows;
            column += 4 - ((rows + 4) % 8);
        }

        if (column < 0)
        {
            column += columns;
            row += 4 - ((columns + 4) % 8);
        }

        cells[(row * columns) + column] =
            (sbyte)((codewords[position] >>> (8 - bit)) & 1);
    }

    private void Utah(int row, int column, int position)
    {
        Module(row - 2, column - 2, position, 1);
        Module(row - 2, column - 1, position, 2);
        Module(row - 1, column - 2, position, 3);
        Module(row - 1, column - 1, position, 4);
        Module(row - 1, column, position, 5);
        Module(row, column - 2, position, 6);
        Module(row, column - 1, position, 7);
        Module(row, column, position, 8);
    }

    private void Corner1(int position)
    {
        Module(rows - 1, 0, position, 1);
        Module(rows - 1, 1, position, 2);
        Module(rows - 1, 2, position, 3);
        Module(0, columns - 2, position, 4);
        Module(0, columns - 1, position, 5);
        Module(1, columns - 1, position, 6);
        Module(2, columns - 1, position, 7);
        Module(3, columns - 1, position, 8);
    }

    private void Corner2(int position)
    {
        Module(rows - 3, 0, position, 1);
        Module(rows - 2, 0, position, 2);
        Module(rows - 1, 0, position, 3);
        Module(0, columns - 4, position, 4);
        Module(0, columns - 3, position, 5);
        Module(0, columns - 2, position, 6);
        Module(0, columns - 1, position, 7);
        Module(1, columns - 1, position, 8);
    }

    private void Corner3(int position)
    {
        Module(rows - 3, 0, position, 1);
        Module(rows - 2, 0, position, 2);
        Module(rows - 1, 0, position, 3);
        Module(0, columns - 2, position, 4);
        Module(0, columns - 1, position, 5);
        Module(1, columns - 1, position, 6);
        Module(2, columns - 1, position, 7);
        Module(3, columns - 1, position, 8);
    }

    private void Corner4(int position)
    {
        Module(rows - 1, 0, position, 1);
        Module(rows - 1, columns - 1, position, 2);
        Module(0, columns - 3, position, 3);
        Module(0, columns - 2, position, 4);
        Module(0, columns - 1, position, 5);
        Module(1, columns - 3, position, 6);
        Module(1, columns - 2, position, 7);
        Module(1, columns - 1, position, 8);
    }
}
