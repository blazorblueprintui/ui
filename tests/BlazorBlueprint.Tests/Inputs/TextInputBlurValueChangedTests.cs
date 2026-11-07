using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Inputs;

/// <summary>
/// The text inputs raise <c>ValueChanged</c> from two places: while typing (Immediate and
/// Debounced) and on blur or Enter (all modes). When typing has already raised the value, the
/// blur that follows must not raise it again: a <c>@bind-Value:after</c> handler would run twice
/// for one edit (issue #605).
/// </summary>
public class TextInputBlurValueChangedTests
{
    public static TheoryData<Type> TextInputs =>
    [
        typeof(BbInput),
        typeof(BbTextarea),
        typeof(BbInputGroupInput),
        typeof(BbInputGroupTextarea),
    ];

    [Theory]
    [MemberData(nameof(TextInputs))]
    public async Task BlurAfterADebouncedRaiseDoesNotRaiseTheSameValueAgain(Type type)
    {
        await RunAsync(type, UpdateTiming.Debounced, async (renderer, input, raised) =>
        {
            await Call(renderer, input, "JsOnInput", "abc");
            await Call(renderer, input, "JsOnChange", "abc");

            Assert.Equal(["abc"], raised);
        });
    }

    [Theory]
    [MemberData(nameof(TextInputs))]
    public async Task BlurAfterAnImmediateRaiseDoesNotRaiseTheSameValueAgain(Type type)
    {
        await RunAsync(type, UpdateTiming.Immediate, async (renderer, input, raised) =>
        {
            await Call(renderer, input, "JsOnInput", "abc");
            await Call(renderer, input, "JsOnChange", "abc");

            Assert.Equal(["abc"], raised);
        });
    }

    [Theory]
    [MemberData(nameof(TextInputs))]
    public async Task BlurBeforeTheDebounceElapsesStillRaisesTheNewValue(Type type)
    {
        await RunAsync(type, UpdateTiming.Debounced, async (renderer, input, raised) =>
        {
            await Call(renderer, input, "JsOnInput", "ab");

            // Typed "c", then left before the debounce fired: blur carries the pending edit.
            await Call(renderer, input, "JsOnChange", "abc");

            Assert.Equal(["ab", "abc"], raised);
        });
    }

    [Theory]
    [MemberData(nameof(TextInputs))]
    public async Task OnChangeModeRaisesOnBlurWhenTheValueChanged(Type type)
    {
        await RunAsync(type, UpdateTiming.OnChange, async (renderer, input, raised) =>
        {
            await Call(renderer, input, "JsOnChange", "abc");

            Assert.Equal(["abc"], raised);
        });
    }

    // -------------------------------------------------------------------------------------

    private static Task Call(ComponentTestRenderer renderer, IComponent input, string method, string? value) =>
        renderer.Dispatcher.InvokeAsync(async () =>
            await (Task)input.GetType().GetMethod(method)!.Invoke(input, [value])!);

    private static async Task RunAsync(
        Type type,
        UpdateTiming timing,
        Func<ComponentTestRenderer, IComponent, List<string?>, Task> body)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();

        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        var raised = new List<string?>();
        var parameters = new Dictionary<string, object?>
        {
            ["UpdateTiming"] = timing,
            ["ValueChanged"] = EventCallback.Factory.Create<string?>(raised, raised.Add),
        };

        var input = await renderer.Dispatcher.InvokeAsync(async () => await renderer.MountTypeAsync(type, parameters));
        await body(renderer, input, raised);
    }
}
