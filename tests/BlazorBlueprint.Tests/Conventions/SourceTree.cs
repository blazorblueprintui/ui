using System.Reflection;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Locates the repository's source directories from the test assembly's location.
/// <para>
/// The convention tests read component markup as text rather than exercising rendered output,
/// because the defects they guard against are omissions in markup — a missing CSS class, a value
/// interpolated without a culture — that a behavioural test cannot see. The logic is correct in
/// both cases; the string is not.
/// </para>
/// </summary>
internal static class SourceTree
{
    private static readonly Lazy<DirectoryInfo> RepoRootLazy = new(FindRepoRoot);

    /// <summary>Every <c>.razor</c> and <c>.cs</c> file in the two component libraries.</summary>
    internal static IReadOnlyList<FileInfo> ComponentSources { get; } = EnumerateSources();

    private static DirectoryInfo FindRepoRoot()
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "BlazorBlueprint.Components")))
            {
                return dir;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root by walking up from the test assembly. " +
            "These tests read component source as text, so they need the working tree — " +
            "they cannot run against packaged assemblies alone.");
    }

    private static List<FileInfo> EnumerateSources()
    {
        string[] roots =
        [
            Path.Combine("src", "BlazorBlueprint.Components", "Components"),
            Path.Combine("src", "BlazorBlueprint.Primitives", "Primitives"),
        ];

        var files = new List<FileInfo>();

        foreach (var relative in roots)
        {
            var root = new DirectoryInfo(Path.Combine(RepoRootLazy.Value.FullName, relative));
            if (!root.Exists)
            {
                continue;
            }

            files.AddRange(root
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(f => f.Extension is ".razor" or ".cs"));
        }

        return files;
    }

    /// <summary>
    /// Groups source files by component, so a component split across <c>Foo.razor</c> and
    /// <c>Foo.razor.cs</c> is judged on both halves together. Markup lives in one file and the
    /// class strings in the other, so inspecting either alone gives the wrong answer.
    /// </summary>
    internal static IEnumerable<(string Component, string Text)> ByComponent()
    {
        return ComponentSources
            .GroupBy(f => f.Name.Split('.')[0], StringComparer.Ordinal)
            .Select(g => (g.Key, string.Join("\n", g.Select(f => File.ReadAllText(f.FullName)))));
    }

    /// <summary>Path relative to the repository root, for readable assertion messages.</summary>
    internal static string RelativePath(FileInfo file) =>
        Path.GetRelativePath(RepoRootLazy.Value.FullName, file.FullName);
}
