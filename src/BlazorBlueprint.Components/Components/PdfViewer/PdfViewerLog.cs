using Microsoft.Extensions.Logging;

namespace BlazorBlueprint.Components;

/// <summary>
/// Source-generated log messages for <see cref="BbPdfViewer"/>. The viewer shows its users a
/// localized message; what PDF.js actually reported goes here.
/// </summary>
internal static partial class PdfViewerLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "The PDF viewer's script could not be loaded, so it cannot show documents.")]
    public static partial void ModuleImportFailed(ILogger logger, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "The PDF viewer could not load the document: {Reason}")]
    public static partial void LoadFailed(ILogger logger, string reason);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "The PDF viewer's {Action} failed: {Reason}")]
    public static partial void ActionFailed(ILogger logger, string action, string reason);
}
