using BlazorBlueprint.Primitives.DataGrid;

namespace BlazorBlueprint.Components;

/// <summary>One atomic batch-save request. Persist the changes transactionally in the callback.</summary>
/// <typeparam name="TData">The row type.</typeparam>
public sealed class DataGridBatchCommitContext<TData> where TData : class
{
    /// <summary>Gets the proposed changes and their original records.</summary>
    public required IReadOnlyList<DataGridEditChange<TData>> Changes { get; init; }

    /// <summary>Gets or sets whether the batch was rejected. Rejection retains all drafts.</summary>
    public bool Cancel { get; set; }
}
