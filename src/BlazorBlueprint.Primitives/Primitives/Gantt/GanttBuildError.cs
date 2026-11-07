namespace BlazorBlueprint.Primitives.Gantt;

/// <summary>
/// Why <see cref="GanttBuilder"/> refused a task list, carried on <see cref="GanttBuildException.Error"/>.
/// </summary>
public enum GanttBuildError
{
    /// <summary>No reason was given. No arguments.</summary>
    Unspecified,

    /// <summary>A task has no identifier. No arguments.</summary>
    MissingId,

    /// <summary>Two tasks share an identifier. Arguments: the identifier.</summary>
    DuplicateId,

    /// <summary>The parent identifiers form a loop. No arguments.</summary>
    ParentLoop,
}
