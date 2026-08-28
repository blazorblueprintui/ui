using System.Globalization;
using System.Text;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Renders DataGrid rows as CSV text, using each column's display value.
/// </summary>
/// <remarks>
/// A column that sets a sort-and-filter selector exports that value, so a cell showing
/// <c>Active</c> for a stored <c>3</c> exports <c>Active</c>. Every other column exports
/// <see cref="IDataGridColumn{TData}.GetValue"/>, which already has the column's format applied.
/// </remarks>
public static class DataGridCsvWriter
{
    /// <summary>
    /// The characters a spreadsheet reads as the start of a formula.
    /// </summary>
    /// <remarks>
    /// A cell beginning with one of these is executed when the file is opened in Excel or Sheets,
    /// so an attacker who can get text into a row can get code run on the machine of whoever
    /// exports and opens the file. Prefixing the cell with an apostrophe makes the spreadsheet
    /// treat it as text. The tab and carriage return are included because both can be used to
    /// shift a formula past a naive check.
    /// </remarks>
    private static readonly char[] FormulaTriggers = { '=', '+', '-', '@', '\t', '\r' };

    /// <summary>
    /// Renders rows as CSV text.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="rows">The rows to export, already filtered and sorted as the user sees them.</param>
    /// <param name="columns">The columns to export, in display order. Pass only the visible ones.</param>
    /// <param name="delimiter">The field delimiter. Defaults to a comma.</param>
    /// <param name="includeHeader">Whether to write a header row of column titles. Defaults to true.</param>
    /// <returns>The CSV text, with CRLF line endings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when rows or columns is null.</exception>
    /// <exception cref="ArgumentException">Thrown when delimiter is null or empty.</exception>
    public static string Write<TData>(
        IEnumerable<TData> rows,
        IReadOnlyList<IDataGridColumn<TData>> columns,
        string delimiter = ",",
        bool includeHeader = true) where TData : class
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columns);

        if (string.IsNullOrEmpty(delimiter))
        {
            throw new ArgumentException("The delimiter cannot be null or empty.", nameof(delimiter));
        }

        var builder = new StringBuilder();

        if (includeHeader)
        {
            AppendRow(builder, columns.Select(c => c.Title ?? c.ColumnId), delimiter);
        }

        foreach (var row in rows)
        {
            AppendRow(builder, columns.Select(c => GetExportText(c, row)), delimiter);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the text a column exports for one row: its sort-and-filter value when it has one,
    /// otherwise its formatted display value.
    /// </summary>
    public static string? GetExportText<TData>(IDataGridColumn<TData> column, TData row) where TData : class
    {
        ArgumentNullException.ThrowIfNull(column);

        var value = column.GetSortAndFilterExpression() != null
            ? column.GetSortAndFilterValue(row)
            : column.GetValue(row);

        return value == null ? null : Convert.ToString(value, CultureInfo.CurrentCulture);
    }

    private static void AppendRow(StringBuilder builder, IEnumerable<string?> cells, string delimiter)
    {
        var first = true;
        foreach (var cell in cells)
        {
            if (!first)
            {
                builder.Append(delimiter);
            }

            builder.Append(EscapeCell(cell, delimiter));
            first = false;
        }

        builder.Append("\r\n");
    }

    /// <summary>
    /// Quotes a cell when it would otherwise break the row, after neutralizing any leading
    /// formula character.
    /// </summary>
    private static string EscapeCell(string? value, string delimiter)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var text = Sanitize(value);

        var needsQuotes = text.Contains('"', StringComparison.Ordinal)
            || text.Contains('\n', StringComparison.Ordinal)
            || text.Contains('\r', StringComparison.Ordinal)
            || text.Contains(delimiter, StringComparison.Ordinal);

        return needsQuotes
            ? string.Concat("\"", text.Replace("\"", "\"\"", StringComparison.Ordinal), "\"")
            : text;
    }

    /// <summary>
    /// Prefixes a cell with an apostrophe when a spreadsheet would read it as a formula.
    /// </summary>
    /// <remarks>
    /// A value that parses as a number is left alone, so a negative amount such as
    /// <c>-1,500.00</c> still exports as a number rather than as text. Anything else starting with
    /// a trigger character is prefixed, which covers the attack shape (<c>=1+1</c>,
    /// <c>-2+3+cmd|...</c>) without spoiling ordinary numeric columns.
    /// </remarks>
    private static string Sanitize(string value)
    {
        if (Array.IndexOf(FormulaTriggers, value[0]) < 0)
        {
            return value;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
        {
            return value;
        }

        return string.Concat("'", value);
    }
}
