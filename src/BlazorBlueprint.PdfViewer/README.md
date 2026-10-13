# BlazorBlueprint.PdfViewer

A PDF viewer for Blazor, built on [PDF.js](https://mozilla.github.io/pdf.js/): paging, zoom,
fit-to-width and download, from a URL or from bytes your app already holds. It works in Server,
WebAssembly and Auto render modes.

It is an add-on to [BlazorBlueprint.Components](https://www.nuget.org/packages/BlazorBlueprint.Components),
in a package of its own because PDF.js is over 2 MB of script and WebAssembly. Apps that don't
show PDFs don't carry it.

## Install

```bash
dotnet add package BlazorBlueprint.PdfViewer
```

Set up BlazorBlueprint.Components first, then add the viewer's stylesheet **after**
`blazorblueprint.css` in `App.razor` (or `index.html`):

```html
<link href="_content/BlazorBlueprint.Components/blazorblueprint.css" rel="stylesheet" />
<link href="_content/BlazorBlueprint.PdfViewer/blazorblueprint-pdfviewer.css" rel="stylesheet" />
```

And the namespace in `_Imports.razor`:

```razor
@using BlazorBlueprint.PdfViewer
```

## Use

```razor
<BbPdfViewer Url="/files/report.pdf" Height="600px" />
```

Documents can also come from bytes, with no URL or request at all:

```razor
<BbPdfViewer @ref="viewer" Height="600px" />

@code {
    private BbPdfViewer? viewer;

    private async Task ShowAsync(byte[] pdf) => await viewer!.LoadDataAsync(pdf);
}
```

## Localization

The viewer's strings use the `PdfViewer.` keys through BlazorBlueprint's `IBbLocalizer`, with
English defaults in `PdfViewerLocalization.Defaults`:

```csharp
builder.Services.AddBlazorBlueprintComponents(localizer =>
{
    localizer.Set("PdfViewer.NextPage", "Page suivante");
});
```

## Licenses

BlazorBlueprint.PdfViewer is Apache-2.0. It bundles PDF.js (Apache-2.0) and PDF.js's image
decoders (OpenJPEG BSD-2-Clause, PDFium JBIG2 BSD-3-Clause, qcms MIT); their notices ship in
`wwwroot/THIRD-PARTY-NOTICES.txt`.
