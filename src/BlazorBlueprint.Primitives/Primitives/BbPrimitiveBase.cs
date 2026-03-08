using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives;

/// <summary>
/// Base class for primitives that compute inline styles and need to merge them
/// with user-provided styles from <see cref="AdditionalAttributes"/>.
/// <para>
/// Blazor's <c>@attributes</c> splatting overwrites any explicit <c>style</c> attribute
/// when the consumer also passes a <c>style</c> through <c>AdditionalAttributes</c>.
/// This base class provides <see cref="MergeStyles"/> and <see cref="FilteredAttributes"/>
/// to safely combine computed and user styles without conflicts.
/// </para>
/// </summary>
public abstract class BbPrimitiveBase : ComponentBase
{
    /// <summary>
    /// Additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Merges a component's computed style with any user-provided <c>style</c>
    /// from <see cref="AdditionalAttributes"/>.
    /// </summary>
    /// <param name="computedStyle">The component's functional style (positioning, sizing, etc.).</param>
    /// <returns>
    /// The merged style string, or <c>null</c> if both the computed style
    /// and user style are empty.
    /// </returns>
    protected string? MergeStyles(string? computedStyle)
    {
        var userStyle = AdditionalAttributes != null
            && AdditionalAttributes.TryGetValue("style", out var s)
                ? s?.ToString()
                : null;

        if (string.IsNullOrEmpty(computedStyle))
        {
            return string.IsNullOrEmpty(userStyle) ? null : userStyle;
        }

        return string.IsNullOrEmpty(userStyle) ? computedStyle : $"{computedStyle}; {userStyle}";
    }

    /// <summary>
    /// Returns <see cref="AdditionalAttributes"/> with the <c>style</c> key removed,
    /// preventing the user's style from overwriting the merged style via <c>@attributes</c>.
    /// </summary>
    protected Dictionary<string, object>? FilteredAttributes
    {
        get
        {
            if (AdditionalAttributes == null || !AdditionalAttributes.ContainsKey("style"))
            {
                return AdditionalAttributes;
            }

            var filtered = new Dictionary<string, object>(AdditionalAttributes);
            filtered.Remove("style");
            return filtered.Count > 0 ? filtered : null;
        }
    }
}
