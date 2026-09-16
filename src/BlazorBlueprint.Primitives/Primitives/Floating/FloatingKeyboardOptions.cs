namespace BlazorBlueprint.Primitives.Floating;

/// <summary>
/// The kind of keyboard behaviour a floating overlay's content needs.
/// </summary>
public enum FloatingKeyboardKind
{
    /// <summary>
    /// A listbox: arrow keys move a <c>data-focused</c> highlight, and the overlay opens scrolled
    /// to the selected option with focus on the listbox itself.
    /// </summary>
    Listbox,

    /// <summary>
    /// A menu: arrow keys move real DOM focus between menu items, with type-ahead.
    /// </summary>
    Menu,
}

/// <summary>
/// Asks <see cref="BbFloatingPortal"/> to wire the content's keyboard behaviour inside the same
/// interop call that opens it.
/// </summary>
/// <remarks>
/// <para>
/// Owners used to do this themselves from the portal's ready callback. On Blazor Server that
/// callback only runs once the browser has acknowledged the render batch carrying the content, so
/// the wiring was a second round trip and anything it awaited was a third: the overlay appeared,
/// and some time later started answering the arrow keys. At a 90ms round trip that gap is plainly
/// visible, and for a listbox it also arrived as a scroll jump rather than as the state the list
/// opened in.
/// </para>
/// <para>
/// Declared here instead, it all happens inside the one call — before the reveal for anything that
/// changes what the first frame looks like, after it for anything that needs focus, which a hidden
/// element cannot take.
/// </para>
/// </remarks>
public sealed class FloatingKeyboardOptions
{
    /// <summary>
    /// Which keyboard behaviour to wire.
    /// </summary>
    public FloatingKeyboardKind Kind { get; init; } = FloatingKeyboardKind.Listbox;

    /// <summary>
    /// The <c>id</c> of the element that owns the keys — the listbox, or the menu container.
    /// </summary>
    public string ContentId { get; init; } = string.Empty;

    /// <summary>
    /// The <c>DotNetObjectReference</c> the handler calls back into for the keys it does not
    /// handle itself, such as Escape and Tab.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The owner's, not the portal's. The portal hands its own reference to the dismissal
    /// listeners it wires, and those call <c>JsOnDismissOutside</c> and <c>JsOnDismissEscape</c> on
    /// the portal — but content that handles Escape on its own element calls methods that live on
    /// the owner. Passing the portal's reference instead gets as far as the first Escape and then
    /// throws
    /// <c>does not contain a public invokable method with [JSInvokableAttribute("JsOnEscapeKey")]</c>.
    /// </para>
    /// <para>
    /// Typed as <see cref="object"/> because <c>DotNetObjectReference&lt;T&gt;</c> has no
    /// non-generic base that serialises; the interop serialiser resolves it by runtime type.
    /// </para>
    /// </remarks>
    public object? CallbackRef { get; init; }

    /// <summary>
    /// <see cref="FloatingKeyboardKind.Listbox"/> only. The currently selected value, so the
    /// matching option is scrolled into view and highlighted before the listbox is revealed. Null
    /// highlights the first enabled option.
    /// </summary>
    public string? SelectedValue { get; init; }

    /// <summary>
    /// <see cref="FloatingKeyboardKind.Menu"/> only. Whether arrow keys wrap from the last item
    /// back to the first. Default is <c>true</c>.
    /// </summary>
    public bool Loop { get; init; } = true;

    /// <summary>
    /// <see cref="FloatingKeyboardKind.Menu"/> only. <c>"vertical"</c> for a dropdown or context
    /// menu, <c>"menubar"</c> to also answer the left and right arrows.
    /// </summary>
    public string Mode { get; init; } = "vertical";

    /// <summary>Menu focus after reveal: "first", "last", "container", or null to preserve focus.</summary>
    public string? InitialFocus { get; init; }
}
