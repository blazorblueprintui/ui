namespace BlazorBlueprint.Components.Utilities;

/// <summary>
/// Manual tests for ClassNames utility.
/// Run this class to verify cn() works correctly.
/// </summary>
public static class ClassNamesTests
{
    private static readonly string[] AbcArray = ["a", "b", "c"];
    private static readonly string[] BcArray = ["b", "c"];

    public static void RunTests()
    {
        Console.WriteLine("=== ClassNames (cn) Utility Tests ===\n");

        // Test 1: Basic concatenation
        Test("Basic concatenation",
            ClassNames.cn("a", "b", "c"),
            "a b c");

        // Test 2: Null handling
        Test("Null handling",
            ClassNames.cn("a", null, "b", null, "c"),
            "a b c");

        // Test 3: Empty string handling
        Test("Empty string handling",
            ClassNames.cn("a", "", "b", "  ", "c"),
            "a b c");

        // Test 4: Conditional classes (false) - using ternary
        Test("Conditional false",
            ClassNames.cn("btn", false ? "active" : null, "px-4"),
            "btn px-4");

        // Test 5: Conditional classes (true) - using ternary
        Test("Conditional true",
            ClassNames.cn("btn", true ? "active" : null, "px-4"),
            "btn active px-4");

        // Test 6: Array support
        Test("Array support",
            ClassNames.cn(AbcArray),
            "a b c");

        // Test 7: Mixed arrays and strings
        Test("Mixed arrays and strings",
            ClassNames.cn("a", BcArray, "d"),
            "a b c d");

        // Test 8: Tailwind conflict - padding
        Test("Tailwind conflict - padding",
            ClassNames.cn("px-4", "px-2"),
            "px-2");

        // Test 9: Tailwind conflict - padding specificity
        Test("Tailwind conflict - padding specificity",
            ClassNames.cn("p-4", "px-2", "py-6"),
            "px-2 py-6");

        // Test 10: Tailwind conflict - colors
        Test("Tailwind conflict - text color",
            ClassNames.cn("text-blue-500", "text-red-600"),
            "text-red-600");

        // Test 11: Tailwind conflict - background
        Test("Tailwind conflict - background",
            ClassNames.cn("bg-white", "bg-gray-100"),
            "bg-gray-100");

        // Test 12: No conflict - different utilities
        Test("No conflict - different utilities",
            ClassNames.cn("px-4", "py-2", "bg-white", "text-black"),
            "px-4 py-2 bg-white text-black");

        // Test 13: Complex real-world example
        Test("Complex real-world example",
            ClassNames.cn(
                "inline-flex items-center justify-center",
                "rounded-md text-sm font-medium",
                "px-4 py-2",
                true ? "bg-primary text-primary-foreground" : null,
                false ? "bg-secondary" : null,
                "px-8" // Should override px-4
            ),
            "inline-flex items-center justify-center rounded-md text-sm font-medium py-2 bg-primary text-primary-foreground px-8");

        // Test 14: Display conflicts
        Test("Display conflicts",
            ClassNames.cn("block", "flex", "inline-block", "grid"),
            "grid");

        // Test 15: Multi-word classes
        Test("Multi-word classes in string",
            ClassNames.cn("px-4 py-2", "bg-white", "px-8"),
            "py-2 bg-white px-8");

        Console.WriteLine("\n=== All Tests Complete ===");
    }

    private static void Test(string testName, string actual, string expected)
    {
        var passed = actual == expected;
        var status = passed ? "✓ PASS" : "✗ FAIL";
        var color = passed ? ConsoleColor.Green : ConsoleColor.Red;

        if (!OperatingSystem.IsBrowser())
        {
            Console.ForegroundColor = color;
        }
        Console.WriteLine($"{status} - {testName}");
        if (!OperatingSystem.IsBrowser())
        {
            Console.ResetColor();
        }

        if (!passed)
        {
            Console.WriteLine($"  Expected: \"{expected}\"");
            Console.WriteLine($"  Actual:   \"{actual}\"");
        }
    }
}
