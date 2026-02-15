using BlazorBlueprint.Demo.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace BlazorBlueprint.Demo.Photino;

//NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
// Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
internal sealed class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.Services.AddBlazorBlueprintDemo();
        builder.Services.AddLogging();

        builder.RootComponents.Add<App>("#app");

        var app = builder.Build();

        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("BlazorBlueprint.Demo.Photino");

        AppDomain.CurrentDomain.UnhandledException += (_, error) => app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());

        app.Run();
    }
}
