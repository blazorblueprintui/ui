namespace BlazorBlueprint.Components;

/// <summary>
/// Builds the attributes a component splats onto its popover trigger button from a caller's
/// <c>TriggerAttributes</c> and <c>TriggerId</c>.
/// <para>
/// The id goes into the same dictionary rather than as its own <c>id="@TriggerId"</c> attribute.
/// Razor applies attributes in source order and the last one wins, null included, so an unset
/// <c>TriggerId</c> written beside the splat would erase an <c>id</c> passed in the attributes.
/// </para>
/// </summary>
internal static class TriggerButtonAttributes
{
    /// <summary>
    /// Returns <paramref name="attributes"/> with <paramref name="triggerId"/> as <c>id</c> when it
    /// is set, overriding any <c>id</c> already there; otherwise <paramref name="attributes"/> as is.
    /// </summary>
    public static Dictionary<string, object>? Merge(Dictionary<string, object>? attributes, string? triggerId)
    {
        if (string.IsNullOrEmpty(triggerId))
        {
            return attributes;
        }

        var merged = attributes is null
            ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(attributes, StringComparer.OrdinalIgnoreCase);
        merged["id"] = triggerId;
        return merged;
    }
}
