# Using BlazorUI NuGet Packages Locally

This guide explains how to use the BlazorUI NuGet packages locally before publishing them to NuGet.org.

## Package Information

### Available Packages (v1.0.0-beta.1)

- **BlazorUI.Primitives** - Headless, unstyled Blazor primitive components
- **BlazorUI.Icons** - Lucide icon library (1,600+ SVG icons)
- **BlazorUI.Components** - Pre-styled components with shadcn/ui design

## Local NuGet Feed Setup

The packages are available in a local NuGet feed at: `C:\LocalNuGet`

### Verify Local Source

Check that the "Local" source is configured:

```bash
dotnet nuget list source
```

You should see:
```
3.  Local [Enabled]
    C:\LocalNuGet
```

### Add Local Source (if needed)

If the source isn't configured, add it:

```bash
dotnet nuget add source "C:\LocalNuGet" --name "Local"
```

## Using the Packages in Your Projects

### 1. Add Package References

Add the packages to your project file (.csproj):

```xml
<ItemGroup>
  <!-- For headless primitives only -->
  <PackageReference Include="BlazorUI.Primitives" Version="1.0.0-beta.1" />

  <!-- For icons -->
  <PackageReference Include="BlazorUI.Icons" Version="1.0.0-beta.1" />

  <!-- For pre-styled components (includes Primitives and Icons) -->
  <PackageReference Include="BlazorUI.Components" Version="1.0.0-beta.1" />
</ItemGroup>
```

### 2. Restore Packages

```bash
dotnet restore
```

The packages will be restored from your local feed.

### 3. Use in Your Code

```razor
@using BlazorUI.Components
@using BlazorUI.Icons

<Button Variant="ButtonVariant.Default">
    <ChevronRight class="mr-2 h-4 w-4" />
    Click Me
</Button>
```

## Updating to New Beta Versions

When you make changes and want to test a new version:

### 1. Update Version in Directory.Build.props

```xml
<Version>1.0.0-beta.2</Version>
```

### 2. Rebuild Packages

```bash
# From repository root
dotnet pack src/BlazorUI.Primitives -c Release
dotnet pack src/BlazorUI.Icons -c Release
dotnet pack src/BlazorUI.Components -c Release
```

### 3. Copy to Local Feed

```bash
cp src/BlazorUI.Primitives/bin/Release/*.nupkg /c/LocalNuGet/
cp src/BlazorUI.Icons/bin/Release/*.nupkg /c/LocalNuGet/
cp src/BlazorUI.Components/bin/Release/*.nupkg /c/LocalNuGet/
```

### 4. Update Your Internal Projects

Update the version in your project's .csproj:

```xml
<PackageReference Include="BlazorUI.Components" Version="1.0.0-beta.2" />
```

Then restore:

```bash
dotnet restore
```

## Clearing NuGet Cache

If you're having issues with package updates not being recognized:

```bash
dotnet nuget locals all --clear
```

## Publishing to NuGet.org

When ready for public release:

### 1. Update to Release Version

In `Directory.Build.props`:

```xml
<Version>1.0.0</Version>
```

### 2. Rebuild Packages

```bash
dotnet pack src/BlazorUI.Primitives -c Release
dotnet pack src/BlazorUI.Icons -c Release
dotnet pack src/BlazorUI.Components -c Release
```

### 3. Push to NuGet.org

```bash
# Get your API key from https://www.nuget.org/account/apikeys
dotnet nuget push src/BlazorUI.Primitives/bin/Release/BlazorUI.Primitives.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push src/BlazorUI.Icons/bin/Release/BlazorUI.Icons.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push src/BlazorUI.Components/bin/Release/BlazorUI.Components.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Troubleshooting

### Package Not Found

1. Verify the package exists in `C:\LocalNuGet`:
   ```bash
   ls /c/LocalNuGet/
   ```

2. Check that the "Local" source is enabled:
   ```bash
   dotnet nuget list source
   ```

3. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   ```

### Version Conflicts

If you're getting version conflicts between packages, ensure all BlazorUI packages use the same version:

```xml
<PackageReference Include="BlazorUI.Primitives" Version="1.0.0-beta.1" />
<PackageReference Include="BlazorUI.Icons" Version="1.0.0-beta.1" />
<PackageReference Include="BlazorUI.Components" Version="1.0.0-beta.1" />
```

## Package Locations

- **Source Code**: `src/BlazorUI.*/`
- **Built Packages**: `src/BlazorUI.*/bin/Release/*.nupkg`
- **Local Feed**: `C:\LocalNuGet/`
- **Version Config**: `Directory.Build.props`

## Need Help?

- Check the main README.md for usage examples
- Review component documentation in the demo app
- Open an issue on GitHub: https://github.com/blazorui-net/ui/issues
