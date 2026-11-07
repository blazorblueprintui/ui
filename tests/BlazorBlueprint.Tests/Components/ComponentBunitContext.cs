using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

internal sealed class ComponentBunitContext : BunitContext
{
    public ComponentBunitContext()
    {
        Services.AddBlazorBlueprintComponents();
    }
}
