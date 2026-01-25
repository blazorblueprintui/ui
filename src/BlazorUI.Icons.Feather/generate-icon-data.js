// Node.js script to convert feather.json (IconifyJSON format) to C# dictionary code

const fs = require('fs');
const path = require('path');

const jsonPath = path.join(__dirname, '../../tools/icon-generation/data/feather-icons.json');
const outputPath = path.join(__dirname, 'Data/FeatherIconData.cs');

console.log('Reading Feather icon data from', jsonPath);
const jsonContent = fs.readFileSync(jsonPath, 'utf8');
const data = JSON.parse(jsonContent);

const allIcons = data.icons;
const iconCount = Object.keys(allIcons).length;
console.log(`Found ${iconCount} icons`);

// Helper function to escape C# strings
function escapeCSharp(str) {
    return str.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}

// Helper function to generate dictionary entries
function generateDictionaryEntries(icons, indent) {
    const entries = [];
    const sortedKeys = Object.keys(icons).sort();

    for (let i = 0; i < sortedKeys.length; i++) {
        const iconName = sortedKeys[i];
        const iconBody = icons[iconName].body;
        const escapedBody = escapeCSharp(iconBody);
        const comma = i < sortedKeys.length - 1 ? ',' : '';
        entries.push(`${indent}["${iconName}"] = "${escapedBody}"${comma}`);
    }

    return entries.join('\n');
}

// Create Data directory if it doesn't exist
const dataDir = path.join(__dirname, 'Data');
if (!fs.existsSync(dataDir)) {
    fs.mkdirSync(dataDir, { recursive: true });
    console.log('Created Data directory');
}

// Build the C# file content
const lines = [];

lines.push('// This file is auto-generated. Do not edit manually.');
lines.push(`// Generated from feather.json on ${new Date().toISOString().split('T')[0]}`);
lines.push('');
lines.push('namespace BlazorUI.Icons.Feather.Data;');
lines.push('');
lines.push('/// <summary>');
lines.push('/// Provides access to Feather icon SVG data.');
lines.push(`/// Contains ${iconCount} icons from the Feather icon set.`);
lines.push('/// </summary>');
lines.push('public static class FeatherIconData');
lines.push('{');
lines.push('    private static readonly IReadOnlyDictionary<string, string> Icons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)');
lines.push('    {');
lines.push(generateDictionaryEntries(allIcons, '        '));
lines.push('    };');
lines.push('');

// GetIcon method
lines.push('    /// <summary>');
lines.push('    /// Retrieves the SVG content for the specified icon name.');
lines.push('    /// </summary>');
lines.push('    /// <param name="name">The name of the icon (case-insensitive).</param>');
lines.push('    /// <returns>The SVG path data for the icon, or null if not found.</returns>');
lines.push('    public static string? GetIcon(string name)');
lines.push('    {');
lines.push('        return Icons.TryGetValue(name, out var svg) ? svg : null;');
lines.push('    }');
lines.push('');

// GetAvailableIcons method
lines.push('    /// <summary>');
lines.push('    /// Gets all available icon names.');
lines.push('    /// </summary>');
lines.push('    /// <returns>An enumerable collection of icon names.</returns>');
lines.push('    public static IEnumerable<string> GetAvailableIcons() => Icons.Keys;');
lines.push('');

// IconExists method
lines.push('    /// <summary>');
lines.push('    /// Checks if an icon with the specified name exists.');
lines.push('    /// </summary>');
lines.push('    /// <param name="name">The name of the icon (case-insensitive).</param>');
lines.push('    /// <returns>True if the icon exists, false otherwise.</returns>');
lines.push('    public static bool IconExists(string name) => Icons.ContainsKey(name);');
lines.push('');

// IconCount property
lines.push('    /// <summary>');
lines.push('    /// Gets the total number of available icons.');
lines.push('    /// </summary>');
lines.push('    public static int IconCount => Icons.Count;');
lines.push('}');

// Write to file
const outputContent = lines.join('\n');
fs.writeFileSync(outputPath, outputContent, 'utf8');

console.log('✓ Generated C# file:', outputPath);
console.log('✓ Total icons:', iconCount);
