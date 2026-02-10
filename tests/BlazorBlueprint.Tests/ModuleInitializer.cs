using System.Runtime.CompilerServices;

namespace BlazorBlueprint.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Verify will use the default settings.
        // Verified snapshots (.verified.txt) are committed to source control.
        // Received snapshots (.received.txt) are generated on test failure for comparison.
    }
}
