using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;
using BlazorBlueprint.Components;

namespace BlazorBlueprint.Demo.Desktop1;

internal static class Program {
    [STAThread]
    private static void Main(string[] args) {
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
        appBuilder.Services
            .AddLogging();

        // Add BlazorBlueprint services to the DI container.
        appBuilder.Services
            .AddBlazorBlueprintComponents();

        // Register the root component for the main window.
        appBuilder.RootComponents.Add<App>("app");

        var app = appBuilder.Build();

        // Customize the native Photino window.
        app.MainBlazorWindow.Window
            .SetIconFile("favicon.ico")
            .SetTitle("BlazorBlueprint PhotinoX Desktop1");

        AppDomain.CurrentDomain.UnhandledException += (_, error) => app.MainBlazorWindow.Window.ShowMessage("Fatal exception", error.ExceptionObject?.ToString() ?? "Unknown fatal exception.");

        app.Run();
    }

}
