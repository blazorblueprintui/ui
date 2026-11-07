namespace BlazorBlueprint.Primitives.Gantt;

/// <summary>
/// A task list <see cref="GanttBuilder"/> cannot turn into a chart.
/// </summary>
/// <remarks>
/// An <see cref="ArgumentException"/>, so existing catch blocks keep working. <see cref="Error"/>
/// and <see cref="Arguments"/> say what is wrong in a form a program can read, so a component can
/// show its own translated sentence instead of the English <see cref="Exception.Message"/>.
/// </remarks>
public sealed class GanttBuildException : ArgumentException
{
    /// <summary>Creates the exception with the reason it was thrown.</summary>
    /// <param name="message">What is wrong, as a sentence.</param>
    /// <param name="paramName">The argument at fault.</param>
    /// <param name="error">What is wrong, for a program to read.</param>
    /// <param name="arguments">The values the message was formatted with.</param>
    internal GanttBuildException(string message, string? paramName, GanttBuildError error, params object[] arguments)
        : base(message, paramName)
    {
        Error = error;
        Arguments = arguments;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence.</param>
    /// <param name="paramName">The argument at fault.</param>
    public GanttBuildException(string message, string? paramName)
        : base(message, paramName)
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence.</param>
    public GanttBuildException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception.</summary>
    public GanttBuildException()
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public GanttBuildException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets what is wrong, for a program to read. <see cref="GanttBuildError.Unspecified"/> when
    /// the exception was created without a reason.
    /// </summary>
    public GanttBuildError Error { get; }

    /// <summary>
    /// Gets the values the message was formatted with, in the order <see cref="GanttBuildError"/>
    /// documents for each reason. Empty when there are none.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; } = [];
}
