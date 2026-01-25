# Contributing to BlazorUI

Thank you for your interest in contributing to BlazorUI! We welcome contributions from the community.

## How to Contribute

### Reporting Issues

If you find a bug or have a feature request, please create an issue on GitHub:

1. Check if the issue already exists
2. Use the issue template if available
3. Provide a clear description and reproduction steps for bugs
4. For feature requests, explain the use case and expected behavior

### Submitting Pull Requests

1. **Fork the repository** and create a new branch from `main`
2. **Make your changes** following our code style guidelines
3. **Test your changes** across different Blazor hosting models (Server, WASM, Hybrid)
4. **Update documentation** if you're adding new features or changing behavior
5. **Submit a pull request** with a clear description of your changes

## Development Setup

### Prerequisites

- **.NET 8 SDK** or later
- **Git** for version control
- **IDE**: Visual Studio 2022, VS Code, or Rider (recommended)
- **Tailwind CSS standalone CLI** (included in demo app, no Node.js required)

### Clone and Build

1. **Clone the repository:**

```bash
git clone https://github.com/blazorui-net/ui.git
cd ui
```

2. **Build the solution:**

```bash
dotnet build
```

This builds all five NuGet packages:
- `BlazorUI.Components` - Styled components
- `BlazorUI.Primitives` - Headless primitives
- `BlazorUI.Icons.Lucide` - Lucide icon library
- `BlazorUI.Icons.Heroicons` - Heroicons library
- `BlazorUI.Icons.Feather` - Feather icon library

### Run the Demo Application

```bash
cd demo/BlazorUI.Demo
dotnet watch run
```

The demo app will be available at `https://localhost:5001`

**Note:** The demo includes Tailwind CSS setup and shows all components with live examples.

### Project Structure

This is a mono-repo containing:

```
BlazorUI/
├── src/
│   ├── BlazorUI.Components/      # Styled components (shadcn/ui design)
│   ├── BlazorUI.Primitives/      # Headless UI primitives
│   ├── BlazorUI.Icons.Lucide/    # Lucide icon integration
│   ├── BlazorUI.Icons.Heroicons/ # Heroicons integration
│   └── BlazorUI.Icons.Feather/   # Feather icon integration
├── demo/
│   └── BlazorUI.Demo/            # Demo Blazor Server app
├── website/
│   └── BlazorUI.Website/         # Documentation website
├── .devflow/                     # DevFlow documentation and standards
├── scripts/                      # Release automation scripts
└── .github/                      # CI/CD workflows
```

### Technology Stack

- **.NET 8 (LTS)** - Target framework
- **Blazor** - Supports Server, WebAssembly, and Hybrid hosting models
- **Tailwind CSS** - Utility-first CSS framework (standalone CLI, no Node.js required)
- **CSS Variables** - Runtime theme switching
- **Lucide Icons** - 1000+ beautiful, consistent icons

For detailed technical architecture, see [.devflow/architecture.md](../.devflow/architecture.md)

## Code Style Guidelines

BlazorUI follows the coding standards documented in `.devflow/constitution.md`. Key guidelines:

### Naming Conventions
- **PascalCase** for public members, types, and namespaces
- **camelCase** for private fields (no underscore prefix)
- Component files: `ComponentName.razor` + `ComponentName.razor.cs`

### Documentation
- Add XML documentation for all public APIs
- Include inline comments for complex logic
- Update README.md for new components

### Code Quality
- Follow Blazor best practices
- Avoid unnecessary re-renders
- Properly dispose of resources
- Use null-safety throughout

### Accessibility
- All components must be WCAG 2.1 AA compliant
- Implement proper ARIA attributes
- Support keyboard navigation
- Test with screen readers

## Testing

Before submitting a pull request, please:

- Test across Blazor Server, WebAssembly, and Hybrid
- Verify cross-browser compatibility (Chrome, Firefox, Edge, Safari)
- Validate accessibility with keyboard navigation
- Check that all components render correctly in light and dark mode

## License

By contributing to BlazorUI, you agree that your contributions will be licensed under the [Apache License 2.0](LICENSE).

This means:
- Your contributions will be freely available to everyone
- You retain copyright to your contributions
- You grant BlazorUI and its users the rights specified in the Apache License 2.0

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Accept constructive criticism gracefully
- Focus on what's best for the community
- Show empathy towards other community members

### Unacceptable Behavior

- Harassment, discrimination, or offensive comments
- Personal attacks or trolling
- Publishing others' private information
- Other conduct that would be inappropriate in a professional setting

## Questions?

If you have questions about contributing, feel free to:

- Open an issue for discussion
- Join the community discussions on GitHub
- Reach out to the maintainers

Thank you for contributing to BlazorUI!
