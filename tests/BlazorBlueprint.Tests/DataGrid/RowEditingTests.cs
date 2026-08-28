using BlazorBlueprint.Primitives.DataGrid;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// Covers the snapshot behind row editing: row edits bind straight to the item, so cancelling has
/// to put the original values back on that same instance.
/// </summary>
public class RowEditingTests
{
    private sealed class Employee
    {
        public string Name { get; set; } = "";

        public int Salary { get; set; }

        public DateTime? Reviewed { get; set; }

        public string Initials => Name.Length > 0 ? Name[..1] : "";

        public string WriteOnly { private get; set; } = "";

        public string ReadWriteOnly() => WriteOnly;
    }

    private sealed class Throwing
    {
        private string backing = "";

        public string Safe { get; set; } = "";

        public string Explodes
        {
            get => throw new InvalidOperationException(backing);
            set => throw new InvalidOperationException(value);
        }
    }

    [Fact]
    public void RestorePutsEveryChangedValueBack()
    {
        var employee = new Employee { Name = "Ana", Salary = 100, Reviewed = new DateTime(2026, 1, 1) };
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";
        employee.Salary = 200;
        employee.Reviewed = null;

        snapshot.Restore();

        Assert.Equal("Ana", employee.Name);
        Assert.Equal(100, employee.Salary);
        Assert.Equal(new DateTime(2026, 1, 1), employee.Reviewed);
    }

    [Fact]
    public void RestoreWritesBackToTheSameInstance()
    {
        // Anything else holding a reference to the row must see the values revert too, which is
        // why the snapshot restores in place rather than handing back a copy.
        var employee = new Employee { Name = "Ana" };
        var alias = employee;
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";
        snapshot.Restore();

        Assert.Same(employee, snapshot.Item);
        Assert.Equal("Ana", alias.Name);
    }

    [Fact]
    public void HasChangesIsFalseUntilSomethingIsEdited()
    {
        var employee = new Employee { Name = "Ana", Salary = 100 };
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        Assert.False(snapshot.HasChanges());

        employee.Salary = 101;

        Assert.True(snapshot.HasChanges());
    }

    [Fact]
    public void HasChangesGoesBackToFalseAfterARestore()
    {
        var employee = new Employee { Name = "Ana" };
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";
        snapshot.Restore();

        Assert.False(snapshot.HasChanges());
    }

    [Fact]
    public void EditingToTheSameValueIsNotAChange()
    {
        var employee = new Employee { Name = "Ana" };
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";
        employee.Name = "Ana";

        Assert.False(snapshot.HasChanges());
    }

    [Fact]
    public void AComputedPropertyIsIgnoredBecauseThereIsNothingToRestore()
    {
        var employee = new Employee { Name = "Ana" };
        var snapshot = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";

        // Initials has no setter, so restoring Name is what puts it right.
        snapshot.Restore();

        Assert.Equal("A", employee.Initials);
    }

    [Fact]
    public void APropertyThatThrowsDoesNotAbandonTheRestore()
    {
        var item = new Throwing { Safe = "before" };
        var snapshot = DataGridRowSnapshot<Throwing>.Capture(item);

        item.Safe = "after";
        snapshot.Restore();

        Assert.Equal("before", item.Safe);
    }

    [Fact]
    public void CaptureRejectsANullItem()
    {
        Assert.Throws<ArgumentNullException>(() => DataGridRowSnapshot<Employee>.Capture(null!));
    }

    [Fact]
    public void TwoSnapshotsOfTheSameItemAreIndependent()
    {
        // Starting a second edit after committing the first must not rewind past the commit.
        var employee = new Employee { Name = "Ana" };
        var first = DataGridRowSnapshot<Employee>.Capture(employee);

        employee.Name = "Bo";
        var second = DataGridRowSnapshot<Employee>.Capture(employee);
        employee.Name = "Cy";

        second.Restore();
        Assert.Equal("Bo", employee.Name);

        first.Restore();
        Assert.Equal("Ana", employee.Name);
    }
}
