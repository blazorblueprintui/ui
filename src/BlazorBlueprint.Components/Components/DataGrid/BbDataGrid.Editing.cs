using System.ComponentModel.DataAnnotations;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

public partial class BbDataGrid<TData> where TData : class
{
    private DataGridEditBuffer<TData>? editBuffer;
    private string? editingColumnId;
    private TData? editModel;
    private TData? cellCheckpoint;
    private readonly ConditionalWeakTable<TData, CellFocusIdentity> cellFocusIds = new();
    private string? returnFocusId;
    private string? pendingFocusId;
    private bool cellHadDraft;
    private bool savingEdits;
    private bool focusEditor;
    private string? editError;
    private IDisposable? editValidation;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Clones a source row into an independent edit model. Required for Cell and Batch modes.
    /// Clone editable nested objects as well. Stable ItemKey values must not be edited.
    /// </summary>
    [Parameter]
    public Func<TData, TData>? EditItemFactory { get; set; }

    /// <summary>Persists a batch transaction. Set Cancel to retain all drafts after rejection.</summary>
    [Parameter]
    public EventCallback<DataGridBatchCommitContext<TData>> OnBatchCommit { get; set; }

    /// <summary>Occurs after the pending batch has been discarded.</summary>
    [Parameter]
    public EventCallback OnBatchCancel { get; set; }

    /// <summary>Gets the pending batch drafts without mutating the source records.</summary>
    public IReadOnlyList<DataGridEditChange<TData>> PendingChanges => editBuffer?.Changes ?? [];

    /// <summary>Gets whether a save callback is currently running.</summary>
    public bool IsSavingEdits => savingEdits;

    /// <summary>Gets the column currently being edited in Cell or Batch mode.</summary>
    public string? EditingColumnId => editingColumnId;

    private bool UsesEditBuffer => EditMode is DataGridEditMode.Cell or DataGridEditMode.Batch;

    private TData DisplayItem(TData item) => editBuffer?.GetDisplayItem(item) ?? item;

    private bool IsCellEditable(IDataGridColumn<TData> column) => UsesEditBuffer && column.EditTemplate != null;

    /// <summary>Starts editing an editable visible column. Pending invalid edits block navigation.</summary>
    public async Task StartCellEditAsync(TData item, string columnId)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!UsesEditBuffer || savingEdits)
        {
            return;
        }

        var column = _columns.Find(c => c.ColumnId == columnId && c.Visible && c.EditTemplate != null)
            ?? throw new ArgumentException("The column must be visible and have an EditTemplate.", nameof(columnId));
        if (IsEditing(item) && editingColumnId == column.ColumnId)
        {
            return;
        }

        if (_editingItem != null && !await CommitEditAsync())
        {
            return;
        }

        var factory = EditItemFactory ?? throw new InvalidOperationException("Cell and Batch editing require EditItemFactory.");
        editBuffer ??= new DataGridEditBuffer<TData>(factory, ItemKey);
        cellHadDraft = !ReferenceEquals(editBuffer.GetDisplayItem(item), item);
        var draft = editBuffer.Begin(item);
        var checkpoint = factory(draft);
        if (checkpoint == null || ReferenceEquals(checkpoint, draft) || ReferenceEquals(checkpoint, item))
        {
            throw new InvalidOperationException("EditItemFactory must return an independent copy for cancellation.");
        }

        _editingItem = item;
        editModel = draft;
        cellCheckpoint = checkpoint;
        editingColumnId = columnId;
        returnFocusId = CellTriggerId(item, columnId);
        _editContext = new EditContext(draft);
        editValidation = _editContext.EnableDataAnnotationsValidation(ServiceProvider);
        editError = null;
        focusEditor = true;
        _stateVersion++;
        StateHasChanged();
    }

    private string CellTriggerId(TData item, string columnId) =>
        $"{gridId}-{cellFocusIds.GetValue(item, _ => new CellFocusIdentity()).Id}-{columnId}";

    private sealed class CellFocusIdentity
    {
        internal string Id { get; } = Guid.NewGuid().ToString("N");
    }

    private async Task<bool> CommitBufferedCellAsync()
    {
        if (savingEdits || _editingItem == null || editModel == null)
        {
            return !savingEdits;
        }

        if (_editContext != null && !_editContext.Validate())
        {
            focusEditor = true;
            _stateVersion++;
            StateHasChanged();
            return false;
        }

        if (ItemKey != null && !Equals(ItemKey(_editingItem), ItemKey(editModel)))
        {
            editError = Localizer["DataGrid.ImmutableKey"];
            _stateVersion++;
            StateHasChanged();
            return false;
        }

        if (EditMode == DataGridEditMode.Batch)
        {
            ClearEditState();
            StateHasChanged();
            return true;
        }

        savingEdits = true;
        _stateVersion++;
        StateHasChanged();
        try
        {
            var context = new DataGridRowCommitContext<TData> { Item = editModel, OriginalItem = _editingItem };
            await OnRowCommit.InvokeAsync(context);
            if (context.Cancel)
            {
                editError = Localizer["DataGrid.SaveRejected"];
                return false;
            }

            ApplyDrafts([new DataGridEditChange<TData> { Item = editModel, OriginalItem = _editingItem }]);
            editBuffer!.Discard(_editingItem);
            ClearEditState();
            await RefreshDataAsync();
            return true;
        }
        catch (Exception)
        {
            // A failed remote save must not discard the draft or terminate a server circuit.
            editError = Localizer["DataGrid.SaveFailed"];
            return false;
        }
        finally
        {
            savingEdits = false;
            _stateVersion++;
            StateHasChanged();
        }
    }

    /// <summary>Validates and saves all staged rows with one batch callback.</summary>
    /// <returns>True if all changes were accepted; false if validation or persistence failed.</returns>
    public async Task<bool> CommitBatchAsync()
    {
        if (EditMode != DataGridEditMode.Batch || savingEdits)
        {
            return false;
        }

        if (_editingItem != null && !await CommitEditAsync())
        {
            return false;
        }

        var changes = PendingChanges;
        if (changes.Count == 0)
        {
            return true;
        }

        foreach (var change in changes)
        {
            if ((ItemKey != null && !Equals(ItemKey(change.OriginalItem), ItemKey(change.Item)))
                || !Validator.TryValidateObject(change.Item, new ValidationContext(change.Item, ServiceProvider, null), [], true))
            {
                editError = Localizer["DataGrid.InvalidBatch"];
                _stateVersion++;
                StateHasChanged();
                return false;
            }
        }

        savingEdits = true;
        editError = null;
        _stateVersion++;
        StateHasChanged();
        try
        {
            var context = new DataGridBatchCommitContext<TData> { Changes = changes };
            await OnBatchCommit.InvokeAsync(context);
            if (context.Cancel)
            {
                editError = Localizer["DataGrid.SaveRejected"];
                return false;
            }

            ApplyDrafts(changes);

            editBuffer!.Clear();
            await RefreshDataAsync();
            return true;
        }
        catch (Exception)
        {
            editError = Localizer["DataGrid.SaveFailed"];
            return false;
        }
        finally
        {
            savingEdits = false;
            _stateVersion++;
            StateHasChanged();
        }
    }

    private void ApplyDrafts(IReadOnlyList<DataGridEditChange<TData>> changes)
    {
        if (ItemKey != null && changes.Any(c => !Equals(ItemKey(c.OriginalItem), ItemKey(c.Item))))
        {
            throw new InvalidOperationException("The row key cannot change during persistence.");
        }
        var originals = changes.Select(c => DataGridRowSnapshot<TData>.Capture(c.OriginalItem)).ToArray();
        try
        {
            foreach (var change in changes)
            {
                DataGridRowSnapshot<TData>.Capture(change.Item).ApplyTo(change.OriginalItem);
            }
        }
        catch
        {
            foreach (var original in originals)
            {
                original.Restore();
            }
            throw;
        }
    }

    /// <summary>Discards active and staged batch drafts. Source records are unchanged.</summary>
    public async Task CancelBatchAsync()
    {
        if (savingEdits)
        {
            return;
        }

        editBuffer?.Clear();
        ClearEditState();
        editError = null;
        await OnBatchCancel.InvokeAsync();
        StateHasChanged();
    }

    private async Task FocusCellEditorAsync()
    {
        if (!focusEditor && pendingFocusId == null)
        {
            return;
        }

        var editor = focusEditor;
        var restore = pendingFocusId;
        focusEditor = false;
        pendingFocusId = null;
        try
        {
            var module = await JsModules.GetAsync(Js, "./_content/BlazorBlueprint.Components/js/datagrid-editing.js");
            await module.InvokeVoidAsync("focusEditor", containerRef, editor ? null : restore);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            // Navigation and prerendering can remove the editor before focus is applied.
        }
    }
}
