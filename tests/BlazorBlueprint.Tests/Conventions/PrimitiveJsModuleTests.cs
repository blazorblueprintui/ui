using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Guards the one round trip that loading the primitive JavaScript is allowed to cost.
/// <para>
/// In Blazor Server an <c>import(...)</c> issued from C# is an instruction posted to the browser
/// that the server then awaits, so every lazily-imported module costs a full circuit round trip —
/// per module, per page load, cached or not. The primitives therefore ship as one bundle and every
/// call site addresses it by a <c>namespace.function</c> identifier.
/// </para>
/// <para>
/// Neither the compiler nor the API surface snapshot can see a JavaScript identifier. A wrong one
/// fails only at runtime, in the browser, as <c>Could not find 'x.y'</c> — and only on the code
/// path that uses it, which for an overlay may be a keyboard interaction nobody clicks through.
/// These tests read the identifiers out of the C# and check them against the modules.
/// </para>
/// </summary>
public class PrimitiveJsModuleTests
{
    private static readonly string PrimitiveJsRoot = Path.Combine(
        SourceTree.RepoRoot.FullName,
        "src", "BlazorBlueprint.Primitives", "wwwroot", "js", "primitives");

    private const string BundleFileName = "bb-primitives.js";

    /// <summary><c>export * as clickOutside from './click-outside.js';</c></summary>
    private static readonly Regex ReExport = new(
        @"^export \* as (?<ns>\w+) from '\./(?<file>[\w-]+)\.js';",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary><c>export function foo</c>, <c>export async function foo</c>, <c>export const foo</c>.</summary>
    private static readonly Regex ModuleExport = new(
        @"^export\s+(?:async\s+)?(?:function|const|let|class)\s+(?<name>\w+)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>An interop call naming a bundle function: <c>InvokeVoidAsync("portal.lockBodyScroll", …)</c>.</summary>
    private static readonly Regex DottedInvoke = new(
        @"Invoke(?:Void)?Async(?:<[^>]*>)?\(\s*\n?\s*""(?<ns>[a-z][\w]*)\.(?<fn>[A-Za-z]\w*)""",
        RegexOptions.Compiled);

    /// <summary><c>document.addEventListener('keydown', …)</c>, however it is spelled.</summary>
    private static readonly Regex DocumentKeydownListener = new(
        @"document\s*\.\s*addEventListener\s*\(\s*['""]keydown['""]",
        RegexOptions.Compiled);

    /// <summary>A C# import of a primitive module by path.</summary>
    private static readonly Regex DirectImport = new(
        @"""\./_content/BlazorBlueprint\.Primitives/js/primitives/(?<file>[\w-]+)\.js""",
        RegexOptions.Compiled);

    [Fact]
    public void TheBundleReExportsEveryPrimitiveModule()
    {
        var bundled = ReExport
            .Matches(File.ReadAllText(Path.Combine(PrimitiveJsRoot, BundleFileName)))
            .Select(m => m.Groups["file"].Value)
            .ToHashSet(StringComparer.Ordinal);

        var onDisk = Directory
            .EnumerateFiles(PrimitiveJsRoot, "*.js")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != Path.GetFileNameWithoutExtension(BundleFileName))
            .ToList();

        var unbundled = onDisk.Where(name => !bundled.Contains(name!)).ToList();

        Assert.True(
            unbundled.Count == 0,
            $"These primitive modules are not re-exported from {BundleFileName}, so reaching them " +
            $"costs an extra circuit round trip on every page load: {string.Join(", ", unbundled)}. " +
            "Add an `export * as <camelCase> from './<file>.js';` line for each.");
    }

    [Fact]
    public void TheBundleNamespaceIsTheFileNameInCamelCase()
    {
        var wrong = ReExport
            .Matches(File.ReadAllText(Path.Combine(PrimitiveJsRoot, BundleFileName)))
            .Where(m => m.Groups["ns"].Value != ToCamelCase(m.Groups["file"].Value))
            .Select(m => $"{m.Groups["ns"].Value} should be {ToCamelCase(m.Groups["file"].Value)}")
            .ToList();

        Assert.True(
            wrong.Count == 0,
            "The namespace must be the module's file name in camelCase — that rule is the only " +
            $"reason a call site is predictable without opening the bundle: {string.Join("; ", wrong)}");
    }

    [Fact]
    public void EveryInteropIdentifierResolvesToAnExportedFunction()
    {
        var namespaces = ReExport
            .Matches(File.ReadAllText(Path.Combine(PrimitiveJsRoot, BundleFileName)))
            .ToDictionary(m => m.Groups["ns"].Value, m => m.Groups["file"].Value, StringComparer.Ordinal);

        var exportsByNamespace = namespaces.ToDictionary(
            pair => pair.Key,
            pair => ModuleExport
                .Matches(File.ReadAllText(Path.Combine(PrimitiveJsRoot, pair.Value + ".js")))
                .Select(m => m.Groups["name"].Value)
                .ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);

        var unresolved = new List<string>();

        foreach (var file in InteropSources())
        {
            foreach (Match match in DottedInvoke.Matches(File.ReadAllText(file.FullName)))
            {
                var ns = match.Groups["ns"].Value;
                var fn = match.Groups["fn"].Value;

                // Identifiers on a cleanup handle returned from JS ("dispose", "apply") carry no
                // namespace and so never reach here. Anything whose prefix is not a bundle
                // namespace belongs to another library's module and is not ours to check.
                if (!exportsByNamespace.TryGetValue(ns, out var exports))
                {
                    continue;
                }

                if (!exports.Contains(fn))
                {
                    unresolved.Add($"{SourceTree.RelativePath(file)}: \"{ns}.{fn}\" — " +
                                   $"{namespaces[ns]}.js exports no {fn}");
                }
            }
        }

        Assert.True(
            unresolved.Count == 0,
            "These interop identifiers would fail at runtime with \"Could not find\":\n  " +
            string.Join("\n  ", unresolved));
    }

    [Fact]
    public void NothingImportsAPrimitiveModuleDirectly()
    {
        var offenders = InteropSources()
            .Where(f => f.Name != "PrimitiveModules.cs")
            .SelectMany(f => DirectImport
                .Matches(File.ReadAllText(f.FullName))
                .Select(m => $"{SourceTree.RelativePath(f)} imports {m.Groups["file"].Value}.js"))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Each of these costs its own circuit round trip on every page load. Use " +
            "PrimitiveModules.GetAsync(JSRuntime) and a namespaced identifier instead:\n  " +
            string.Join("\n  ", offenders));
    }

    [Fact]
    public void OnlyTheEscapeStackWatchesEscapeAtTheDocument()
    {
        // Escape must dismiss the topmost overlay, not every open one. escape-keydown.js keeps a
        // stack behind a single document listener and is the only thing that can order them; a
        // second module adding its own document listener is invisible to that ordering, and a
        // popover inside a dialog then closes both at once. Overlays whose content holds focus
        // (Select, menus) handle Escape on their own container and stop it propagating, which is
        // why a container-level listener is fine and a document-level one is not.
        // A document-level keydown listener is only a problem when it acts on Escape.
        // element-utils.js watches for Tab to tell a keyboard arrival from a mouse one, and
        // keyboard-shortcuts.js serves application shortcuts; neither looks at Escape.
        var offenders = Directory
            .EnumerateFiles(PrimitiveJsRoot, "*.js")
            .Where(file => Path.GetFileName(file) != "escape-keydown.js")
            .Select(file => (Name: Path.GetFileName(file), Text: File.ReadAllText(file)))
            .Where(m => DocumentKeydownListener.IsMatch(m.Text) && m.Text.Contains("Escape", StringComparison.Ordinal))
            .Select(m => m.Name)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "These modules add their own document-level keydown listener, so Escape would bypass " +
            $"the ordering in escape-keydown.js: {string.Join(", ", offenders)}. Register with " +
            "escapeKeydown.initialize(ref, id, method) instead, or handle the key on your own " +
            "container and call stopPropagation().");
    }

    /// <summary>
    /// Every library source that could call into the primitive modules. Wider than
    /// <see cref="SourceTree.ComponentSources"/>, which stops at the component folders — the
    /// services that own positioning, focus and shortcuts sit outside them.
    /// </summary>
    private static IEnumerable<FileInfo> InteropSources()
    {
        string[] roots =
        [
            Path.Combine("src", "BlazorBlueprint.Components"),
            Path.Combine("src", "BlazorBlueprint.Primitives"),
        ];

        return roots
            .Select(relative => new DirectoryInfo(Path.Combine(SourceTree.RepoRoot.FullName, relative)))
            .Where(dir => dir.Exists)
            .SelectMany(dir => dir.EnumerateFiles("*", SearchOption.AllDirectories))
            .Where(f => f.Extension is ".razor" or ".cs")
            .Where(f => !f.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => !f.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static string ToCamelCase(string kebab)
    {
        var parts = kebab.Split('-');
        return parts[0] + string.Concat(parts.Skip(1).Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
