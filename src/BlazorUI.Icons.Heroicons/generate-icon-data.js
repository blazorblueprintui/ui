// Node.js script to convert heroicons.json (IconifyJSON format) to C# dictionary code
// Heroicons have 4 variants: outline (default), solid, mini (20-solid), micro (16-solid)

const fs = require('fs');
const path = require('path');

const jsonPath = path.join(__dirname, '../../tools/icon-generation/data/heroicons.json');
const outputPath = path.join(__dirname, 'Data/HeroIconData.cs');

console.log('Reading Heroicons icon data from', jsonPath);
const jsonContent = fs.readFileSync(jsonPath, 'utf8');
const data = JSON.parse(jsonContent);

const allIcons = data.icons;
const totalIconCount = Object.keys(allIcons).length;
console.log(`Found ${totalIconCount} total icon entries (across all variants)`);

// Group icons by variant based on suffix
const outlineIcons = {};
const solidIcons = {};
const miniIcons = {};
const microIcons = {};

for (const [iconName, iconData] of Object.entries(allIcons)) {
    const iconBody = iconData.body;

    if (iconName.endsWith('-16-solid')) {
        // Micro variant (16x16)
        const baseName = iconName.replace(/-16-solid$/, '');
        microIcons[baseName] = iconBody;
    } else if (iconName.endsWith('-20-solid')) {
        // Mini variant (20x20)
        const baseName = iconName.replace(/-20-solid$/, '');
        miniIcons[baseName] = iconBody;
    } else if (iconName.endsWith('-solid')) {
        // Solid variant (24x24)
        const baseName = iconName.replace(/-solid$/, '');
        solidIcons[baseName] = iconBody;
    } else {
        // Outline variant (24x24) - no suffix
        outlineIcons[iconName] = iconBody;
    }
}

const outlineCount = Object.keys(outlineIcons).length;
const solidCount = Object.keys(solidIcons).length;
const miniCount = Object.keys(miniIcons).length;
const microCount = Object.keys(microIcons).length;

console.log('Grouped icons by variant:');
console.log(`  Outline: ${outlineCount} icons`);
console.log(`  Solid: ${solidCount} icons`);
console.log(`  Mini: ${miniCount} icons`);
console.log(`  Micro: ${microCount} icons`);

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
        const iconBody = icons[iconName];
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
lines.push(`// Generated from heroicons.json on ${new Date().toISOString().split('T')[0]}`);
lines.push('');
lines.push('namespace BlazorUI.Icons.Heroicons.Data;');
lines.push('');
lines.push('/// <summary>');
lines.push('/// Icon variant for Heroicons.');
lines.push('/// </summary>');
lines.push('public enum HeroIconVariant');
lines.push('{');
lines.push('    /// <summary>Outline variant (24x24, stroke-based)</summary>');
lines.push('    Outline,');
lines.push('');
lines.push('    /// <summary>Solid variant (24x24, filled)</summary>');
lines.push('    Solid,');
lines.push('');
lines.push('    /// <summary>Mini variant (20x20, filled)</summary>');
lines.push('    Mini,');
lines.push('');
lines.push('    /// <summary>Micro variant (16x16, filled)</summary>');
lines.push('    Micro');
lines.push('}');
lines.push('');
lines.push('/// <summary>');
lines.push('/// Provides access to Heroicons SVG data.');
lines.push(`/// Contains ${totalIconCount} total icons across 4 variants.`);
lines.push('/// </summary>');
lines.push('public static class HeroIconData');
lines.push('{');

// Outline dictionary
lines.push('    private static readonly IReadOnlyDictionary<string, string> OutlineIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)');
lines.push('    {');
lines.push(generateDictionaryEntries(outlineIcons, '        '));
lines.push('    };');
lines.push('');

// Solid dictionary
lines.push('    private static readonly IReadOnlyDictionary<string, string> SolidIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)');
lines.push('    {');
lines.push(generateDictionaryEntries(solidIcons, '        '));
lines.push('    };');
lines.push('');

// Mini dictionary
lines.push('    private static readonly IReadOnlyDictionary<string, string> MiniIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)');
lines.push('    {');
lines.push(generateDictionaryEntries(miniIcons, '        '));
lines.push('    };');
lines.push('');

// Micro dictionary
lines.push('    private static readonly IReadOnlyDictionary<string, string> MicroIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)');
lines.push('    {');
lines.push(generateDictionaryEntries(microIcons, '        '));
lines.push('    };');
lines.push('');

// GetIcon method
lines.push('    /// <summary>');
lines.push('    /// Retrieves the SVG content for the specified icon name and variant.');
lines.push('    /// </summary>');
lines.push('    /// <param name="name">The name of the icon (case-insensitive).</param>');
lines.push('    /// <param name="variant">The icon variant.</param>');
lines.push('    /// <returns>The SVG path data for the icon, or null if not found.</returns>');
lines.push('    public static string? GetIcon(string name, HeroIconVariant variant)');
lines.push('    {');
lines.push('        var dictionary = variant switch');
lines.push('        {');
lines.push('            HeroIconVariant.Outline => OutlineIcons,');
lines.push('            HeroIconVariant.Solid => SolidIcons,');
lines.push('            HeroIconVariant.Mini => MiniIcons,');
lines.push('            HeroIconVariant.Micro => MicroIcons,');
lines.push('            _ => OutlineIcons');
lines.push('        };');
lines.push('');
lines.push('        return dictionary.TryGetValue(name, out var svg) ? svg : null;');
lines.push('    }');
lines.push('');

// GetAvailableIcons method
lines.push('    /// <summary>');
lines.push('    /// Gets all available icon names for a specific variant.');
lines.push('    /// </summary>');
lines.push('    /// <param name="variant">The icon variant.</param>');
lines.push('    /// <returns>An enumerable collection of icon names.</returns>');
lines.push('    public static IEnumerable<string> GetAvailableIcons(HeroIconVariant variant)');
lines.push('    {');
lines.push('        return variant switch');
lines.push('        {');
lines.push('            HeroIconVariant.Outline => OutlineIcons.Keys,');
lines.push('            HeroIconVariant.Solid => SolidIcons.Keys,');
lines.push('            HeroIconVariant.Mini => MiniIcons.Keys,');
lines.push('            HeroIconVariant.Micro => MicroIcons.Keys,');
lines.push('            _ => OutlineIcons.Keys');
lines.push('        };');
lines.push('    }');
lines.push('');

// IconExists method
lines.push('    /// <summary>');
lines.push('    /// Checks if an icon with the specified name exists in the given variant.');
lines.push('    /// </summary>');
lines.push('    /// <param name="name">The name of the icon (case-insensitive).</param>');
lines.push('    /// <param name="variant">The icon variant.</param>');
lines.push('    /// <returns>True if the icon exists, false otherwise.</returns>');
lines.push('    public static bool IconExists(string name, HeroIconVariant variant)');
lines.push('    {');
lines.push('        return variant switch');
lines.push('        {');
lines.push('            HeroIconVariant.Outline => OutlineIcons.ContainsKey(name),');
lines.push('            HeroIconVariant.Solid => SolidIcons.ContainsKey(name),');
lines.push('            HeroIconVariant.Mini => MiniIcons.ContainsKey(name),');
lines.push('            HeroIconVariant.Micro => MicroIcons.ContainsKey(name),');
lines.push('            _ => OutlineIcons.ContainsKey(name)');
lines.push('        };');
lines.push('    }');
lines.push('');

// IconCount properties
lines.push('    /// <summary>');
lines.push('    /// Gets the total number of available icons across all variants.');
lines.push('    /// </summary>');
lines.push('    public static int TotalIconCount => OutlineIcons.Count + SolidIcons.Count + MiniIcons.Count + MicroIcons.Count;');
lines.push('');
lines.push('    /// <summary>');
lines.push('    /// Gets the number of outline icons.');
lines.push('    /// </summary>');
lines.push('    public static int OutlineIconCount => OutlineIcons.Count;');
lines.push('');
lines.push('    /// <summary>');
lines.push('    /// Gets the number of solid icons.');
lines.push('    /// </summary>');
lines.push('    public static int SolidIconCount => SolidIcons.Count;');
lines.push('');
lines.push('    /// <summary>');
lines.push('    /// Gets the number of mini icons.');
lines.push('    /// </summary>');
lines.push('    public static int MiniIconCount => MiniIcons.Count;');
lines.push('');
lines.push('    /// <summary>');
lines.push('    /// Gets the number of micro icons.');
lines.push('    /// </summary>');
lines.push('    public static int MicroIconCount => MicroIcons.Count;');
lines.push('}');

// Write to file
const outputContent = lines.join('\n');
fs.writeFileSync(outputPath, outputContent, 'utf8');

console.log('✓ Generated C# file:', outputPath);
console.log('✓ Total icons:', totalIconCount);
console.log(`  - Outline: ${outlineCount}`);
console.log(`  - Solid: ${solidCount}`);
console.log(`  - Mini: ${miniCount}`);
console.log(`  - Micro: ${microCount}`);
