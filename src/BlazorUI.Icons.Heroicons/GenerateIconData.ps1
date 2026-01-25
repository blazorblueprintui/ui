# PowerShell script to convert heroicons.json (IconifyJSON format) to C# dictionary code
# Heroicons have 4 variants: outline (default), solid, mini (20-solid), micro (16-solid)

$jsonPath = Join-Path $PSScriptRoot "..\..\tools\icon-generation\data\heroicons.json"
$outputPath = Join-Path $PSScriptRoot "Data\HeroIconData.cs"

Write-Host "Reading Heroicons icon data from $jsonPath..."
$json = Get-Content -Path $jsonPath -Raw | ConvertFrom-Json

$allIcons = $json.icons.PSObject.Properties
$totalIconCount = $allIcons.Count
Write-Host "Found $totalIconCount total icon entries (across all variants)"

# Create Data directory if it doesn't exist
$dataDir = Join-Path $PSScriptRoot "Data"
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir | Out-Null
    Write-Host "Created Data directory"
}

# Group icons by variant based on suffix
$outlineIcons = @{}
$solidIcons = @{}
$miniIcons = @{}
$microIcons = @{}

foreach ($icon in $allIcons) {
    $iconName = $icon.Name
    $iconBody = $icon.Value.body

    if ($iconName -match '-16-solid$') {
        # Micro variant (16x16)
        $baseName = $iconName -replace '-16-solid$', ''
        $microIcons[$baseName] = $iconBody
    }
    elseif ($iconName -match '-20-solid$') {
        # Mini variant (20x20)
        $baseName = $iconName -replace '-20-solid$', ''
        $miniIcons[$baseName] = $iconBody
    }
    elseif ($iconName -match '-solid$') {
        # Solid variant (24x24)
        $baseName = $iconName -replace '-solid$', ''
        $solidIcons[$baseName] = $iconBody
    }
    else {
        # Outline variant (24x24) - no suffix
        $outlineIcons[$iconName] = $iconBody
    }
}

$outlineCount = $outlineIcons.Count
$solidCount = $solidIcons.Count
$miniCount = $miniIcons.Count
$microCount = $microIcons.Count

Write-Host "Grouped icons by variant:"
Write-Host "  Outline: $outlineCount icons"
Write-Host "  Solid: $solidCount icons"
Write-Host "  Mini: $miniCount icons"
Write-Host "  Micro: $microCount icons"

# Helper function to generate dictionary entries
function Write-IconDictionary {
    param(
        [System.Text.StringBuilder]$sb,
        [hashtable]$icons,
        [string]$indent
    )

    $sortedIcons = $icons.GetEnumerator() | Sort-Object Name
    $lastIndex = $sortedIcons.Count - 1
    $currentIndex = 0

    foreach ($entry in $sortedIcons) {
        $iconName = $entry.Name
        $iconBody = $entry.Value

        # Escape double quotes and backslashes in the SVG
        $escapedBody = $iconBody -replace '\\', '\\' -replace '"', '\"'

        # Add comma except for last item
        $comma = if ($currentIndex -eq $lastIndex) { "" } else { "," }

        [void]$sb.AppendLine("$indent[`"$iconName`"] = `"$escapedBody`"$comma")

        $currentIndex++
    }
}

# Start building the C# file
$sb = New-Object System.Text.StringBuilder

[void]$sb.AppendLine("// This file is auto-generated. Do not edit manually.")
[void]$sb.AppendLine("// Generated from heroicons.json on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace BlazorUI.Icons.Heroicons.Data;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// Icon variant for Heroicons.")
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public enum HeroIconVariant")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    /// <summary>Outline variant (24x24, stroke-based)</summary>")
[void]$sb.AppendLine("    Outline,")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Solid variant (24x24, filled)</summary>")
[void]$sb.AppendLine("    Solid,")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Mini variant (20x20, filled)</summary>")
[void]$sb.AppendLine("    Mini,")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Micro variant (16x16, filled)</summary>")
[void]$sb.AppendLine("    Micro")
[void]$sb.AppendLine("}")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// Provides access to Heroicons SVG data.")
[void]$sb.AppendLine("/// Contains $totalIconCount total icons across 4 variants.")
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public static class HeroIconData")
[void]$sb.AppendLine("{")

# Outline dictionary
[void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, string> OutlineIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("    {")
Write-IconDictionary -sb $sb -icons $outlineIcons -indent "        "
[void]$sb.AppendLine("    };")
[void]$sb.AppendLine("")

# Solid dictionary
[void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, string> SolidIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("    {")
Write-IconDictionary -sb $sb -icons $solidIcons -indent "        "
[void]$sb.AppendLine("    };")
[void]$sb.AppendLine("")

# Mini dictionary
[void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, string> MiniIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("    {")
Write-IconDictionary -sb $sb -icons $miniIcons -indent "        "
[void]$sb.AppendLine("    };")
[void]$sb.AppendLine("")

# Micro dictionary
[void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, string> MicroIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("    {")
Write-IconDictionary -sb $sb -icons $microIcons -indent "        "
[void]$sb.AppendLine("    };")
[void]$sb.AppendLine("")

# GetIcon method
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Retrieves the SVG content for the specified icon name and variant.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <param name=`"name`">The name of the icon (case-insensitive).</param>")
[void]$sb.AppendLine("    /// <param name=`"variant`">The icon variant.</param>")
[void]$sb.AppendLine("    /// <returns>The SVG path data for the icon, or null if not found.</returns>")
[void]$sb.AppendLine("    public static string? GetIcon(string name, HeroIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        var dictionary = variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            HeroIconVariant.Outline => OutlineIcons,")
[void]$sb.AppendLine("            HeroIconVariant.Solid => SolidIcons,")
[void]$sb.AppendLine("            HeroIconVariant.Mini => MiniIcons,")
[void]$sb.AppendLine("            HeroIconVariant.Micro => MicroIcons,")
[void]$sb.AppendLine("            _ => OutlineIcons")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        return dictionary.TryGetValue(name, out var svg) ? svg : null;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")

# GetAvailableIcons method
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets all available icon names for a specific variant.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <param name=`"variant`">The icon variant.</param>")
[void]$sb.AppendLine("    /// <returns>An enumerable collection of icon names.</returns>")
[void]$sb.AppendLine("    public static IEnumerable<string> GetAvailableIcons(HeroIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        return variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            HeroIconVariant.Outline => OutlineIcons.Keys,")
[void]$sb.AppendLine("            HeroIconVariant.Solid => SolidIcons.Keys,")
[void]$sb.AppendLine("            HeroIconVariant.Mini => MiniIcons.Keys,")
[void]$sb.AppendLine("            HeroIconVariant.Micro => MicroIcons.Keys,")
[void]$sb.AppendLine("            _ => OutlineIcons.Keys")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")

# IconExists method
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Checks if an icon with the specified name exists in the given variant.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <param name=`"name`">The name of the icon (case-insensitive).</param>")
[void]$sb.AppendLine("    /// <param name=`"variant`">The icon variant.</param>")
[void]$sb.AppendLine("    /// <returns>True if the icon exists, false otherwise.</returns>")
[void]$sb.AppendLine("    public static bool IconExists(string name, HeroIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        return variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            HeroIconVariant.Outline => OutlineIcons.ContainsKey(name),")
[void]$sb.AppendLine("            HeroIconVariant.Solid => SolidIcons.ContainsKey(name),")
[void]$sb.AppendLine("            HeroIconVariant.Mini => MiniIcons.ContainsKey(name),")
[void]$sb.AppendLine("            HeroIconVariant.Micro => MicroIcons.ContainsKey(name),")
[void]$sb.AppendLine("            _ => OutlineIcons.ContainsKey(name)")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")

# IconCount properties
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the total number of available icons across all variants.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int TotalIconCount => OutlineIcons.Count + SolidIcons.Count + MiniIcons.Count + MicroIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the number of outline icons.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int OutlineIconCount => OutlineIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the number of solid icons.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int SolidIconCount => SolidIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the number of mini icons.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int MiniIconCount => MiniIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the number of micro icons.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int MicroIconCount => MicroIcons.Count;")
[void]$sb.AppendLine("}")

# Write to file
$sb.ToString() | Out-File -FilePath $outputPath -Encoding UTF8
Write-Host "✓ Generated C# file: $outputPath"
Write-Host "✓ Total icons: $totalIconCount"
Write-Host "  - Outline: $outlineCount"
Write-Host "  - Solid: $solidCount"
Write-Host "  - Mini: $miniCount"
Write-Host "  - Micro: $microCount"
