namespace BlazorBlueprint.Components;

/// <summary>A timed appointment. Start and End are instants; recurrence follows TimeZoneId wall time.</summary>
public sealed class SchedulerEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    /// <summary>An IANA time zone, such as America/New_York. Defaults to UTC.</summary>
    public string TimeZoneId { get; set; } = "UTC";
    public List<string> ResourceIds { get; set; } = [];
    /// <summary>An RFC 5545 RRULE without the RRULE: prefix. Null means no recurrence.</summary>
    public string? RecurrenceRule { get; set; }
    /// <summary>Original occurrence start instants omitted from this series.</summary>
    public HashSet<DateTimeOffset> ExcludedStarts { get; set; } = [];
    /// <summary>Series ID for an independently edited occurrence.</summary>
    public string? SeriesId { get; set; }
    /// <summary>Original start instant of an independently edited occurrence.</summary>
    public DateTimeOffset? RecurrenceId { get; set; }

    /// <summary>Creates an independent editor copy, including resource and exception collections.</summary>
    public SchedulerEvent Clone() => new()
    {
        Id = Id, Title = Title, Start = Start, End = End, TimeZoneId = TimeZoneId,
        ResourceIds = [.. ResourceIds], RecurrenceRule = RecurrenceRule,
        ExcludedStarts = [.. ExcludedStarts], SeriesId = SeriesId, RecurrenceId = RecurrenceId
    };
}

/// <summary>A resource displayed as its own scheduler lane.</summary>
public sealed record SchedulerResource(string Id, string Title);

/// <summary>A materialized occurrence. Event refers to the source series or standalone event.</summary>
public sealed record SchedulerOccurrence(SchedulerEvent Event, DateTimeOffset Start, DateTimeOffset End);

public enum SchedulerView { Day, Week, WorkWeek }
public enum SchedulerEditScope { Occurrence, Series }
public enum SchedulerChangeKind { Create, Update, Delete }
public enum SchedulerAmbiguousTimeResolution { Earlier, Later }

/// <summary>Proposed event changes. Persist Events atomically, or set Cancel to retain the editor.</summary>
public sealed class SchedulerChangeContext
{
    public required SchedulerChangeKind Kind { get; init; }
    public required SchedulerEditScope Scope { get; init; }
    public required SchedulerEvent Event { get; init; }
    public DateTimeOffset? OccurrenceStart { get; init; }
    public required IReadOnlyList<SchedulerEvent> Events { get; init; }
    public bool Cancel { get; set; }
}
