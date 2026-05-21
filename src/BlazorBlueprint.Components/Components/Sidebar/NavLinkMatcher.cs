using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorBlueprint.Components;

/// <summary>
/// Replicates the active-route matching used by <see cref="NavLink"/> so that sidebar
/// menu buttons can compute their own active state and expose it through a reliable
/// <c>data-active</c> attribute. The algorithm is intentionally identical to
/// <see cref="NavLink"/> so the <c>Match</c> parameter behaves exactly as users expect.
/// </summary>
internal static class NavLinkMatcher
{
    /// <summary>
    /// Determines whether <paramref name="href"/> matches the current location, using the
    /// same rules as <see cref="NavLink"/>.
    /// </summary>
    /// <param name="navigationManager">Used to resolve <paramref name="href"/> to an absolute URI.</param>
    /// <param name="currentUriAbsolute">The current absolute URI to match against.</param>
    /// <param name="href">The link target. A null or empty value never matches.</param>
    /// <param name="match">How the URL should be matched.</param>
    public static bool IsMatch(
        NavigationManager navigationManager,
        string currentUriAbsolute,
        string? href,
        NavLinkMatch match)
    {
        if (string.IsNullOrEmpty(href))
        {
            return false;
        }

        var hrefAbsolute = navigationManager.ToAbsoluteUri(href).AbsoluteUri;

        if (EqualsHrefExactlyOrIfTrailingSlashAdded(currentUriAbsolute, hrefAbsolute))
        {
            return true;
        }

        return match == NavLinkMatch.Prefix
            && IsStrictlyPrefixWithSeparator(currentUriAbsolute, hrefAbsolute);
    }

    private static bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute, string hrefAbsolute)
    {
        if (string.Equals(currentUriAbsolute, hrefAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Special case: a link to "/path/" is also active at "/path" (with no trailing
        // slash), because servers commonly serve the same page for both.
        if (currentUriAbsolute.Length == hrefAbsolute.Length - 1
            && hrefAbsolute[^1] == '/'
            && hrefAbsolute.StartsWith(currentUriAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool IsStrictlyPrefixWithSeparator(string value, string prefix)
    {
        var prefixLength = prefix.Length;

        if (value.Length <= prefixLength)
        {
            return false;
        }

        // Only match when there is a separator character at the end of the prefix or
        // right after it: "/abc" is a prefix of "/abc/def" but not "/abcdef".
        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && (prefixLength == 0
                || !IsUnreservedCharacter(prefix[prefixLength - 1])
                || !IsUnreservedCharacter(value[prefixLength]));
    }

    private static bool IsUnreservedCharacter(char c)
        => char.IsLetterOrDigit(c) || c is '-' or '.' or '_' or '~';
}
