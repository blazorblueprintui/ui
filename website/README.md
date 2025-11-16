# BlazorUI Website

The official marketing and documentation website for BlazorUI - a beautiful component library for Blazor.

## Overview

This is a standalone Blazor Web App (.NET 8) that serves as the commercial website for BlazorUI. It showcases the component library, provides documentation, and helps developers get started with BlazorUI.

**Live Site:** https://www.blazorui.net

## Features

- **Modern Landing Page**: Hero section, features showcase, and component previews
- **Component Gallery**: Browse and explore all styled components with live examples
- **Primitives Gallery**: Explore unstyled, headless components for custom designs
- **Documentation**: Installation guides, getting started tutorials, and API documentation
- **Responsive Design**: Works beautifully on desktop, tablet, and mobile devices
- **Dark Mode Support**: Built-in theme switching with CSS variables

## Technology Stack

- **Blazor .NET 8**: Latest Blazor Web App model with Interactive Server Components
- **Tailwind CSS**: Utility-first CSS framework for styling
- **BlazorUI Components**: Built using the BlazorUI component library itself (dogfooding!)
- **OKLCH Color Space**: Modern color system for better color perception

## Project Structure

```
BlazorUI.Website/
├── Pages/
│   ├── Index.razor                    # Landing page
│   ├── Components/
│   │   └── Index.razor               # Component gallery
│   ├── Primitives/
│   │   └── Index.razor               # Primitives gallery
│   └── Docs/
│       ├── Introduction.razor        # Introduction to BlazorUI
│       ├── Installation.razor        # Installation guide
│       └── GettingStarted.razor      # Getting started tutorial
├── Shared/
│   ├── Layout.razor                  # Main layout
│   ├── NavMenu.razor                 # Navigation header
│   └── Footer.razor                  # Footer
├── wwwroot/
│   ├── css/
│   │   ├── app-input.css            # Tailwind input file
│   │   └── app.css                  # Compiled Tailwind CSS
│   └── styles/
│       └── theme.css                # Theme CSS variables
├── App.razor                         # Root component
├── Program.cs                        # Application startup
└── tailwind.config.js               # Tailwind configuration
```

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Tailwind CSS standalone CLI (for building styles)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/blazorui-net/ui.git
   cd blazorui/website/BlazorUI.Website
   ```

2. **Install Tailwind CSS CLI** (one-time setup)

   Download the Tailwind CSS standalone CLI for your platform:
   - **Windows**: https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
   - **macOS (Apple Silicon)**: https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-arm64
   - **macOS (Intel)**: https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-x64
   - **Linux**: https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64

   Rename the downloaded file to `tailwindcss.exe` (Windows) or `tailwindcss` (macOS/Linux) and place it in the `website/BlazorUI.Website/` directory.

   Make it executable on macOS/Linux:
   ```bash
   chmod +x website/BlazorUI.Website/tailwindcss
   ```

3. **Run the application** (CSS builds automatically)

   The Tailwind CSS will build automatically when you run the application thanks to the MSBuild target in the `.csproj` file.
   ```bash
   dotnet run
   ```

5. **Open in browser**

   Navigate to `https://localhost:5001` or `http://localhost:5000`

## Development

### Automatic Tailwind CSS Build

The project includes a build target that **automatically compiles Tailwind CSS before each build**:

```xml
<Target Name="BuildTailwindCSS" BeforeTargets="BeforeBuild">
  <Message Text="Building Tailwind CSS..." Importance="high" />
  <Exec Command="$(MSBuildProjectDirectory)\tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css" />
</Target>
```

This means:
- ✅ **No manual build step required** - CSS is compiled automatically when you run `dotnet run`, `dotnet build`, or `dotnet watch`
- ✅ **Works in Visual Studio** - CSS rebuilds automatically when you build the project
- ✅ **Works in CI/CD** - CSS compiles as part of the build process

### Development with Watch Mode (Optional)

For faster development with instant CSS updates, you can optionally run Tailwind in watch mode in a separate terminal:

```bash
# Windows
.\tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css --watch

# macOS/Linux
./tailwindcss -i wwwroot/css/app-input.css -o wwwroot/css/app.css --watch
```

Then run the Blazor app with hot reload:
```bash
dotnet watch
```

### Adding New Pages

1. Create a new `.razor` file in the appropriate folder under `Pages/`
2. Add the `@page` directive with the route
3. Update navigation in `NavMenu.razor` if needed

### Customizing Theme

Theme colors are defined using CSS variables in `wwwroot/styles/theme.css`. You can customize:

- Colors (light and dark mode)
- Border radius
- Font families
- Shadows
- And more...

## Deployment

### Azure Static Web Apps

The website can be deployed to Azure Static Web Apps:

1. Create an Azure Static Web App resource
2. Configure the build settings:
   - App location: `website/BlazorUI.Website`
   - Output location: `wwwroot`
3. Deploy via GitHub Actions or Azure CLI

### Docker

Build and run with Docker:

```bash
# Build image
docker build -t blazorui-website .

# Run container
docker run -p 8080:8080 blazorui-website
```

### Static File Hosting

For static file hosting (Netlify, Vercel, etc.), publish as Blazor WebAssembly:

1. Convert to WebAssembly hosting model
2. Run `dotnet publish -c Release`
3. Deploy the contents of `bin/Release/net8.0/publish/wwwroot`

## Contributing

Contributions are welcome! Please see the main repository's contributing guidelines.

## License

This project is part of the BlazorUI library. See the main repository for license information.

## Links

- **Main Repository**: https://github.com/blazorui-net/ui
- **Documentation**: https://www.blazorui.net/docs/introduction
- **NuGet Packages**:
  - BlazorUI.Primitives
  - BlazorUI.Components
  - BlazorUI.Icons.Lucide

## Support

- **Issues**: https://github.com/blazorui-net/ui/issues
- **Discussions**: https://github.com/blazorui-net/ui/discussions

---

Built with ❤️ using BlazorUI
