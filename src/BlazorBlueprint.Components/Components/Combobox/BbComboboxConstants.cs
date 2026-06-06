namespace BlazorBlueprint.Components;

/// <summary>
/// Shared, type-independent constants for the Combobox component family.
/// </summary>
internal static class BbComboboxConstants
{
    /// <summary>
    /// Name of the cascading flag the parent uses to mark its hidden, render-nothing
    /// registration pass so <c>BbComboboxItem</c> children register their display text
    /// on initial load without producing interactive DOM. Lives on a non-generic type so
    /// it can be referenced from a <see cref="Microsoft.AspNetCore.Components.CascadingParameterAttribute"/>
    /// name without involving the parent's type parameter.
    /// </summary>
    public const string RegistrationScopeName = "BbComboboxRegistrationOnly";
}
