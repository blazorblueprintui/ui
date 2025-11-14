# PowerShell script to convert lucide.json to C# dictionary code

$jsonPath = Join-Path $PSScriptRoot "lucide.json"
$outputPath = Join-Path $PSScriptRoot "Data\LucideIconData.cs"

Write-Host "Reading Lucide icon data from $jsonPath..."
$json = Get-Content -Path $jsonPath -Raw | ConvertFrom-Json

$iconCount = $json.icons.PSObject.Properties.Count
Write-Host "Found $iconCount icons"

# Create Data directory if it doesn't exist
$dataDir = Join-Path $PSScriptRoot "Data"
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir | Out-Null
    Write-Host "Created Data directory"
}

# Start building the C# file
$sb = New-Object System.Text.StringBuilder

[void]$sb.AppendLine("// This file is auto-generated. Do not edit manually.")
[void]$sb.AppendLine("// Generated from lucide.json on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace ShadcnBlazor.Icons.Lucide.Data;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/// <summary>")
[void]$sb.AppendLine("/// Provides access to Lucide icon SVG data.")
[void]$sb.AppendLine("/// Contains {0} icons from the Lucide icon set." -f $iconCount)
[void]$sb.AppendLine("/// </summary>")
[void]$sb.AppendLine("public static class LucideIconData")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    private static readonly IReadOnlyDictionary<string, string> Icons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("    {")

# Add each icon to the dictionary
$iconProperties = $json.icons.PSObject.Properties | Sort-Object Name
$lastIndex = $iconProperties.Count - 1
$currentIndex = 0

foreach ($icon in $iconProperties) {
    $iconName = $icon.Name
    $iconBody = $icon.Value.body

    # Escape double quotes and backslashes in the SVG
    $escapedBody = $iconBody -replace '\\', '\\' -replace '"', '\"'

    # Add comma except for last item
    $comma = if ($currentIndex -eq $lastIndex) { "" } else { "," }

    [void]$sb.AppendLine("        [`"$iconName`"] = `"$escapedBody`"$comma")

    $currentIndex++

    if ($currentIndex % 100 -eq 0) {
        Write-Host "Processed $currentIndex / $iconCount icons..."
    }
}

[void]$sb.AppendLine("    };")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Retrieves the SVG content for the specified icon name.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <param name=`"name`">The name of the icon (case-insensitive).</param>")
[void]$sb.AppendLine("    /// <returns>The SVG path data for the icon, or null if not found.</returns>")
[void]$sb.AppendLine("    public static string? GetIcon(string name)")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        return Icons.TryGetValue(name, out var svg) ? svg : null;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets all available icon names.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <returns>An enumerable collection of icon names.</returns>")
[void]$sb.AppendLine("    public static IEnumerable<string> GetAvailableIcons() => Icons.Keys;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Checks if an icon with the specified name exists.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    /// <param name=`"name`">The name of the icon (case-insensitive).</param>")
[void]$sb.AppendLine("    /// <returns>True if the icon exists, false otherwise.</returns>")
[void]$sb.AppendLine("    public static bool IconExists(string name) => Icons.ContainsKey(name);")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    /// <summary>")
[void]$sb.AppendLine("    /// Gets the total number of available icons.")
[void]$sb.AppendLine("    /// </summary>")
[void]$sb.AppendLine("    public static int IconCount => Icons.Count;")
[void]$sb.AppendLine("}")

# Write to file
$sb.ToString() | Out-File -FilePath $outputPath -Encoding UTF8
Write-Host "✓ Generated C# file: $outputPath"
Write-Host "✓ Total icons: $iconCount"
