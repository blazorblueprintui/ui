namespace BlazorBlueprint.Tests.ApiSurface;

public class PdfViewerApiSurfaceTests
{
    [Fact]
    public Task PdfViewerApiSurfaceMatchesBaseline()
    {
        var assembly = typeof(BlazorBlueprint.PdfViewer.BbPdfViewer).Assembly;
        var apiSurface = ApiSurfaceGenerator.Generate(assembly);
        return Verify(apiSurface);
    }
}
