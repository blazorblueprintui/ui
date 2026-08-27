// Triggers a browser download for text generated on the .NET side.
//
// The text is turned into a Blob and handed to a temporary anchor, which is the only route that
// works in every supported browser without a server round trip. The object URL is revoked on the
// next tick: revoking it synchronously cancels the download in Safari.
export function downloadText(fileName, text, mimeType) {
    const blob = new Blob([text], { type: mimeType || 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName || 'download.txt';
    anchor.style.display = 'none';

    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);

    setTimeout(() => URL.revokeObjectURL(url), 0);
}
