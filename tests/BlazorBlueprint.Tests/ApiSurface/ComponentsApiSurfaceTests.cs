namespace BlazorBlueprint.Tests.ApiSurface;

public class ComponentsApiSurfaceTests
{
    [Fact]
    public Task Components_api_surface_matches_baseline()
    {
        var assembly = typeof(BlazorBlueprint.Components.Button.Button).Assembly;
        var apiSurface = ApiSurfaceGenerator.Generate(assembly);
        return Verify(apiSurface);
    }
}
