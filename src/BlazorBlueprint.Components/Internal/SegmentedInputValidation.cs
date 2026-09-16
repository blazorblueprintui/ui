using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

internal sealed class SegmentedInputValidation(Action onValidationChanged) : IDisposable
{
    private EditContext? context;
    private FieldIdentifier fieldIdentifier;
    private ValidationMessageStore? messages;
    private string? error;
    private string? expressionName;

    internal bool IsInvalid => error != null || (context?.GetValidationMessages(fieldIdentifier).Any() ?? false);
    internal string? GetName(string? name) => name ?? expressionName;

    internal void Update<T>(EditContext? editContext, Expression<Func<T>>? expression)
    {
        var nextField = expression == null ? default : FieldIdentifier.Create(expression);
        if (context == editContext && fieldIdentifier.Equals(nextField))
        {
            return;
        }
        Detach();
        fieldIdentifier = nextField;
        expressionName = expression == null ? null : ExpressionPathFormatter.FormatLambda(expression);
        context = expression == null ? null : editContext;
        if (context != null)
        {
            messages = new ValidationMessageStore(context);
            context.OnValidationStateChanged += HandleValidationChanged;
            if (error != null)
            {
                messages.Add(fieldIdentifier, error);
            }
        }
    }

    internal void SetError(string? value)
    {
        if (error == value) { return; }
        error = value;
        messages?.Clear();
        if (value != null)
        {
            messages?.Add(fieldIdentifier, value);
        }
        context?.NotifyValidationStateChanged();
    }

    internal void NotifyFieldChanged() => context?.NotifyFieldChanged(fieldIdentifier);
    private void HandleValidationChanged(object? sender, ValidationStateChangedEventArgs args) => onValidationChanged();

    private void Detach()
    {
        if (context != null)
        {
            context.OnValidationStateChanged -= HandleValidationChanged;
            messages?.Clear();
            context.NotifyValidationStateChanged();
        }
        messages = null;
        context = null;
    }

    public void Dispose() => Detach();
}
