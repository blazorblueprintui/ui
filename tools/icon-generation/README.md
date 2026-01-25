# Icon Generation Tools

This folder contains the source data used to generate the C# icon data files for BlazorUI's icon libraries.

## Overview

BlazorUI provides three icon library packages, each wrapping a popular open-source icon set:

| Package | Icon Set | Icons | License | Source |
|---------|----------|-------|---------|--------|
| `BlazorUI.Icons.Lucide` | [Lucide](https://lucide.dev/) | 1,640+ | ISC | [GitHub](https://github.com/lucide-icons/lucide) |
| `BlazorUI.Icons.Heroicons` | [Heroicons](https://heroicons.com/) | 1,288 | MIT | [GitHub](https://github.com/tailwindlabs/heroicons) |
| `BlazorUI.Icons.Feather` | [Feather](https://feathericons.com/) | 286 | MIT | [GitHub](https://github.com/feathericons/feather) |

## Folder Structure

```
tools/icon-generation/
├── README.md           # This file
└── data/
    ├── feather-icons.json   # Feather icons in Iconify JSON format
    └── heroicons.json       # Heroicons in Iconify JSON format
```

**Note:** Lucide icons use a `lucide.json` file located directly in `src/BlazorUI.Icons.Lucide/`.

## Data Format

The JSON files use the [Iconify JSON format](https://iconify.design/docs/types/iconify-json.html), which includes:

- Icon metadata (name, author, license)
- SVG path data for each icon
- Default dimensions and attributes

## Generation Scripts

Each icon library has its own generation script that converts the JSON data into C# code:

| Icon Library | Script | Output |
|--------------|--------|--------|
| Lucide | `src/BlazorUI.Icons.Lucide/GenerateIconData.ps1` | `Data/LucideIconData.cs` |
| Heroicons | `src/BlazorUI.Icons.Heroicons/generate-icon-data.js` | `Data/HeroIconData.cs` |
| Feather | `src/BlazorUI.Icons.Feather/generate-icon-data.js` | `Data/FeatherIconData.cs` |

### Running the Scripts

**Lucide (PowerShell):**
```powershell
cd src/BlazorUI.Icons.Lucide
./GenerateIconData.ps1
```

**Heroicons (Node.js):**
```bash
cd src/BlazorUI.Icons.Heroicons
node generate-icon-data.js
```

**Feather (Node.js):**
```bash
cd src/BlazorUI.Icons.Feather
node generate-icon-data.js
```

## Updating Icons

To update to a newer version of an icon set:

1. **Download the latest Iconify JSON** from the icon set's repository or [Iconify](https://github.com/iconify/icon-sets)
2. **Replace the JSON file** in `tools/icon-generation/data/` (or `src/BlazorUI.Icons.Lucide/` for Lucide)
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
