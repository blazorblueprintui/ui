namespace BlazorBlueprint.Components;

/// <summary>
/// Why a copy did not happen.
/// </summary>
/// <remarks>
/// Before #466 a failed copy was invisible: the component showed its copied state and the
/// clipboard stayed empty. With an asynchronous source a refusal is much more likely, so the
/// outcome is reported rather than swallowed.
/// </remarks>
public enum CopyTextFailure
{
    /// <summary>
    /// The browser refused the write. Most often the transient user activation had expired,
    /// which is what happens when the text takes too long to produce on a browser without
    /// promise-aware <c>ClipboardItem</c> support.
    /// </summary>
    Refused = 0,

    /// <summary>
    /// There was nothing to copy — no <c>Value</c>, and the func returned null or empty.
    /// </summary>
    NoValue = 1,
}
