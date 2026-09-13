using BlazorBlueprint.Primitives.Services;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Services;

public class NativeOverlayServiceTests
{
    private static NativeOverlayService CreateService(OverlayRenderingStrategy globalDefault)
    {
        var options = new OverlayRenderingOptions { DefaultStrategy = globalDefault };
        return new NativeOverlayService(new StubJsRuntime(), options);
    }

    [Fact]
    public void RequestedStrategyOverridesGlobalDefault()
    {
        var service = CreateService(globalDefault: OverlayRenderingStrategy.JavaScript);

        var resolved = service.ResolveStrategy(OverlayRenderingStrategy.Native);

        Assert.Equal(OverlayRenderingStrategy.Native, resolved);
    }

    [Fact]
    public void NullUsesGlobalDefaultNative()
    {
        var service = CreateService(globalDefault: OverlayRenderingStrategy.Native);

        var resolved = service.ResolveStrategy(null);

        Assert.Equal(OverlayRenderingStrategy.Native, resolved);
    }

    [Fact]
    public void NullUsesGlobalDefaultJavaScript()
    {
        var service = CreateService(globalDefault: OverlayRenderingStrategy.JavaScript);

        var resolved = service.ResolveStrategy(null);

        Assert.Equal(OverlayRenderingStrategy.JavaScript, resolved);
    }

    private sealed class StubJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            throw new JSDisconnectedException("stub");

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            throw new JSDisconnectedException("stub");
    }
}
