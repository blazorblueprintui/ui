using System.Reflection;

namespace BlazorBlueprint.Tests.Performance;

// Exercise processing paths without a browser renderer; these checks count work rather than
// timing the machine running the test. Public APIs remain unchanged.
internal static class ComponentProbe
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    internal static object? Call(object component, string method, params object?[] args) =>
        component.GetType().GetMethod(method, Flags)!.Invoke(component, args);

    internal static T Field<T>(object component, string field) =>
        (T)component.GetType().GetField(field, Flags)!.GetValue(component)!;

    internal static void SetField(object component, string field, object value) =>
        component.GetType().GetField(field, Flags)!.SetValue(component, value);
}
