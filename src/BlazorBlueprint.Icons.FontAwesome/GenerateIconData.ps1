# PowerShell script to convert Iconify Font Awesome 6 JSON sets to C# dictionary code.
# Font Awesome Free has 3 variants distributed as separate Iconify icon sets:
#   - fa6-solid.json   -> FontAwesomeIconVariant.Solid
#   - fa6-regular.json -> FontAwesomeIconVariant.Regular
#   - fa6-brands.json  -> FontAwesomeIconVariant.Brands
#
# Drop the three JSON files (from the @iconify-json/fa6-* npm packages, or
# https://github.com/iconify/icon-sets/tree/master/json) into
# tools/icon-generation/data/ before running this script.

$dataRoot = Join-Path $PSScriptRoot "..\..\tools\icon-generation\data"
$outputPath = Join-Path $PSScriptRoot "Data\FontAwesomeIconData.cs"

$variantFiles = [ordered]@{
    "Solid"   = "fa6-solid.json"
    "Regular" = "fa6-regular.json"
    "Brands"  = "fa6-brands.json"
}

# Load each Iconify set and collect (name -> entry { width, height, body }).
function Read-IconifySet {
    param([string]$jsonPath)

    if (!(Test-Path $jsonPath)) {
        Write-Warning "Missing icon set: $jsonPath - this variant will be emitted as empty."
        return @{ Icons = @{}; Count = 0 }
    }

    $json = Get-Content -Path $jsonPath -Raw | ConvertFrom-Json

    # Iconify JSON exposes default dimensions at the top level; individual icons
    # can override either with their own width/height fields.
    $defaultWidth  = if ($json.width)  { [int]$json.width  } else { 512 }
    $defaultHeight = if ($json.height) { [int]$json.height } else { 512 }

    $icons = @{}
    foreach ($prop in $json.icons.PSObject.Properties) {
        $name = $prop.Name
        $icon = $prop.Value

        $w = if ($icon.PSObject.Properties.Name -contains 'width' -and $icon.width)  { [int]$icon.width  } else { $defaultWidth  }
        $h = if ($icon.PSObject.Properties.Name -contains 'height' -and $icon.height) { [int]$icon.height } else { $defaultHeight }

        $icons[$name] = [pscustomobject]@{
            Width  = $w
            Height = $h
            Body   = $icon.body
        }
    }

    return @{ Icons = $icons; Count = $icons.Count }
}

# Ensure the Data directory exists.
$dataDir = Join-Path $PSScriptRoot "Data"
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir | Out-Null
}

# Read all three sets.
$variantData = [ordered]@{}
foreach ($variant in $variantFiles.Keys) {
    $jsonPath = Join-Path $dataRoot $variantFiles[$variant]
    Write-Host "Reading $variant from $jsonPath..."
    $variantData[$variant] = Read-IconifySet -jsonPath $jsonPath
    Write-Host "  -> $($variantData[$variant].Count) icons"
}

$totalCount = 0
foreach ($v in $variantData.Values) { $totalCount += $v.Count }

# Emit one dictionary block per variant.
function Write-IconDictionary {
    param(
        [System.Text.StringBuilder]$sb,
        [hashtable]$icons,
        [string]$indent
    )

    $sortedIcons = $icons.GetEnumerator() | Sort-Object Name
    $count = $sortedIcons.Count
    $i = 0

    foreach ($entry in $sortedIcons) {
        $iconName = $entry.Name
        $icon = $entry.Value

        # Escape backslashes and double quotes for C# verbatim-free string literals.
        $escapedBody = $icon.Body -replace '\\', '\\' -replace '"', '\"'

        $comma = if ($i -eq ($count - 1)) { "" } else { "," }
        [void]$sb.AppendLine("$indent[`"$iconName`"] = new FontAwesomeIconEntry($($icon.Width), $($icon.Height), `"$escapedBody`")$comma")
        $i++
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("// This file is auto-generated. Do not edit manually.")
[void]$sb.AppendLine("// Generated from fa6-solid.json, fa6-regular.json, fa6-brands.json on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace BlazorBlueprint.Icons.FontAwesome.Data;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// Icon variant for Font Awesome Free.")
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public enum FontAwesomeIconVariant")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    /// <summary>Solid variant (filled glyphs, the most common Font Awesome style)</summary>")
[void]$sb.AppendLine("    Solid,")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Regular variant (outline glyphs, fewer icons available in the Free tier)</summary>")
[void]$sb.AppendLine("    Regular,")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Brands variant (logos for third-party services and products)</summary>")
[void]$sb.AppendLine("    Brands")
[void]$sb.AppendLine("}")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// A single Font Awesome icon entry: SVG body plus intrinsic dimensions used to build the viewBox.")
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public sealed record FontAwesomeIconEntry(int Width, int Height, string Body);")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// Provides access to Font Awesome Free SVG data.")
[void]$sb.AppendLine("/// Contains $totalCount total icons across 3 variants.")
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public static class FontAwesomeIconData")
[void]$sb.AppendLine("{")

foreach ($variant in $variantData.Keys) {
    $fieldName = "${variant}Icons"
    [void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, FontAwesomeIconEntry> $fieldName = new Dictionary<string, FontAwesomeIconEntry>(StringComparer.OrdinalIgnoreCase)")
    [void]$sb.AppendLine("    {")
    Write-IconDictionary -sb $sb -icons $variantData[$variant].Icons -indent "        "
    [void]$sb.AppendLine("    };")
    [void]$sb.AppendLine("")
}

[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Retrieves the icon entry for the specified icon name and variant.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static FontAwesomeIconEntry? GetIcon(string name, FontAwesomeIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        var dictionary = variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Solid => SolidIcons,")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Regular => RegularIcons,")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Brands => BrandsIcons,")
[void]$sb.AppendLine("            _ => SolidIcons")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        return dictionary.TryGetValue(name, out var entry) ? entry : null;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Gets all available icon names for a specific variant.</summary>")
[void]$sb.AppendLine("    public static IEnumerable<string> GetAvailableIcons(FontAwesomeIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        return variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Solid => SolidIcons.Keys,")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Regular => RegularIcons.Keys,")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Brands => BrandsIcons.Keys,")
[void]$sb.AppendLine("            _ => SolidIcons.Keys")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Checks whether an icon with the specified name exists in the given variant.</summary>")
[void]$sb.AppendLine("    public static bool IconExists(string name, FontAwesomeIconVariant variant)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        return variant switch")
[void]$sb.AppendLine("        {")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Solid => SolidIcons.ContainsKey(name),")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Regular => RegularIcons.ContainsKey(name),")
[void]$sb.AppendLine("            FontAwesomeIconVariant.Brands => BrandsIcons.ContainsKey(name),")
[void]$sb.AppendLine("            _ => SolidIcons.ContainsKey(name)")
[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Gets the total number of available icons across all variants.</summary>")
[void]$sb.AppendLine("    public static int TotalIconCount => SolidIcons.Count + RegularIcons.Count + BrandsIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Gets the number of Solid icons.</summary>")
[void]$sb.AppendLine("    public static int SolidIconCount => SolidIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Gets the number of Regular icons.</summary>")
[void]$sb.AppendLine("    public static int RegularIconCount => RegularIcons.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>Gets the number of Brands icons.</summary>")
[void]$sb.AppendLine("    public static int BrandsIconCount => BrandsIcons.Count;")
[void]$sb.AppendLine("}")

$sb.ToString() | Out-File -FilePath $outputPath -Encoding UTF8
Write-Host ""
Write-Host "Generated C# file: $outputPath"
Write-Host "Total icons: $totalCount"
foreach ($variant in $variantData.Keys) {
    Write-Host "  $variant`: $($variantData[$variant].Count)"
}
