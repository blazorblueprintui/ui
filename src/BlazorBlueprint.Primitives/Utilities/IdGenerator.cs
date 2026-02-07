namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Thread-safe utility for generating unique component IDs.
/// Used for ARIA attributes and DOM element identification.
/// </summary>
public static class IdGenerator
{
    private static int _counter;

    /// <summary>
    /// Generates a unique ID with the specified prefix.
    /// Thread-safe using Interlocked operations.
    /// </summary>
    /// <param name="prefix">The prefix for the ID (default: "shadcn").</param>
    /// <returns>A unique ID string in the format "prefix-{counter}".</returns>
    public static string GenerateId(string prefix = "shadcn")
    {
        var id = Interlocked.Increment(ref _counter);
        return $"{prefix}-{id}";
    }

    /// <summary>
    /// Generates multiple unique IDs with the same prefix.
    /// </summary>
    /// <param name="count">Number of IDs to generate.</param>
    /// <param name="prefix">The prefix for the IDs (default: "shadcn").</param>
    /// <returns>An array of unique ID strings.</returns>
    public static string[] GenerateIds(int count, string prefix = "shadcn")
    {
        var ids = new string[count];
        for (var i = 0; i < count; i++)
        {
            ids[i] = GenerateId(prefix);
        }
        return ids;
    }

    /// <summary>
    /// Resets the counter to zero.
    /// WARNING: Only use in testing scenarios.
    /// </summary>
    internal static void Reset() =>
        Interlocked.Exchange(ref _counter, 0);
}
