# Icon Generation Tools

This folder contains the source data and generation scripts used to produce the C# icon data files for BlazorBlueprint's icon libraries.

## Overview

BlazorBlueprint provides three icon library packages, each wrapping a popular open-source icon set:

| Package | Icon Set | Icons | License | Source |
|---------|----------|-------|---------|--------|
| `BlazorBlueprint.Icons.Lucide` | [Lucide](https://lucide.dev/) | 1,640+ | ISC | [GitHub](https://github.com/lucide-icons/lucide) |
| `BlazorBlueprint.Icons.Heroicons` | [Heroicons](https://heroicons.com/) | 1,288 | MIT | [GitHub](https://github.com/tailwindlabs/heroicons) |
| `BlazorBlueprint.Icons.Feather` | [Feather](https://feathericons.com/) | 286 | MIT | [GitHub](https://github.com/feathericons/feather) |

## Folder Structure

```
tools/icon-generation/
├── README.md              # This file
├── generate-lucide.js     # Lucide icon generation script
├── generate-heroicons.js  # Heroicons icon generation script
├── generate-feather.js    # Feather icon generation script
└── data/
    ├── feather-icons.json # Feather icons in Iconify JSON format
    ├── heroicons.json     # Heroicons in Iconify JSON format
    └── lucide.json        # Lucide icons in Iconify JSON format
```

## Data Format

The JSON files use the [Iconify JSON format](https://iconify.design/docs/types/iconify-json.html), which includes:

- Icon metadata (name, author, license)
- SVG path data for each icon
- Default dimensions and attributes

## Generation Scripts

Each icon library has its own Node.js generation script that converts the JSON data into C# code:

| Icon Library | Script | Output |
|--------------|--------|--------|
| Lucide | `tools/icon-generation/generate-lucide.js` | `src/BlazorBlueprint.Icons.Lucide/Data/LucideIconData.cs` |
| Heroicons | `tools/icon-generation/generate-heroicons.js` | `src/BlazorBlueprint.Icons.Heroicons/Data/HeroIconData.cs` |
| Feather | `tools/icon-generation/generate-feather.js` | `src/BlazorBlueprint.Icons.Feather/Data/FeatherIconData.cs` |

### Running the Scripts

All scripts are run from the `tools/icon-generation/` directory:

```bash
cd tools/icon-generation

# Generate one
node generate-lucide.js
node generate-heroicons.js
node generate-feather.js
```

## Updating Icons

To update to a newer version of an icon set:

1. **Download the latest Iconify JSON** from the icon set's repository or [Iconify](https://github.com/iconify/icon-sets)
2. **Replace the JSON file** in `tools/icon-generation/data/`
3. **Run the generation script** for that icon library
4. **Test** that the icons render correctly
5. **Commit** the updated JSON and generated C# files

## Generated Code

The generation scripts produce static C# classes with:

- A dictionary mapping icon names to SVG path data
- `GetIcon(name)` - Retrieve SVG content by name
- `GetAvailableIcons()` - List all available icon names
- `IconExists(name)` - Check if an icon exists
- `IconCount` - Total number of icons

For Heroicons, the generated code also includes variant support (Outline, Solid, Mini, Micro).
