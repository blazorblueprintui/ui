using BlazorBlueprint.Primitives.Extensions;
using BlazorBlueprint.Primitives.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorBlueprint.Tests.Services;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RegistersNativeOverlayService()
    {
        var services = new ServiceCollection();

        services.AddBlazorBlueprintPrimitives();

        Assert.Contains(services, d => d.ServiceType == typeof(INativeOverlayService));
        Assert.Contains(services, d => d.Lifetime == ServiceLifetime.Scoped && d.ServiceType == typeof(INativeOverlayService));
    }

    [Fact]
    public void RegistersOverlayRenderingOptionsAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddBlazorBlueprintPrimitives();

        Assert.Contains(services, d => d.ServiceType == typeof(OverlayRenderingOptions) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void ConfigureOverlaysSetsGlobalDefault()
    {
        var services = new ServiceCollection();

        services.AddBlazorBlueprintPrimitives(o => o.DefaultStrategy = OverlayRenderingStrategy.Native);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<OverlayRenderingOptions>();
        Assert.Equal(OverlayRenderingStrategy.Native, options.DefaultStrategy);
    }
}
