namespace BlazorBlueprint.Tests.ApiSurface;

public class PrimitivesApiSurfaceTests
{
    [Fact]
    public Task Primitives_api_surface_matches_baseline()
    {
        var assembly = typeof(BlazorBlueprint.Primitives.Checkbox.Checkbox).Assembly;
        var apiSurface = ApiSurfaceGenerator.Generate(assembly);
        return Verify(apiSurface);
    }
}
