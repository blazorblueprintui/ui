using BlazorBlueprint.Primitives.Utilities;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

public class LazyChildLoaderTests
{
    private sealed class Node
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
    }

    [Fact]
    public async Task LoadAsyncReturnsChildren()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };

        var result = await loader.LoadAsync("1", parent, _ =>
            Task.FromResult<IEnumerable<Node>>(new List<Node>
            {
                new() { Id = "1.1", Name = "Child 1" },
                new() { Id = "1.2", Name = "Child 2" }
            }));

        Assert.Equal(2, result.Count);
        Assert.Equal("1.1", result[0].Id);
    }

    [Fact]
    public async Task LoadAsyncCachesResult()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };
        var callCount = 0;

        Task<IEnumerable<Node>> LoaderFn(Node _)
        {
            callCount++;
            return Task.FromResult<IEnumerable<Node>>(new List<Node>
            {
                new() { Id = "1.1", Name = "Child" }
            });
        }

        await loader.LoadAsync("1", parent, LoaderFn);
        await loader.LoadAsync("1", parent, LoaderFn);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task LoadAsyncSetsLoadingState()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };
        var tcs = new TaskCompletionSource<IEnumerable<Node>>();

        var task = loader.LoadAsync("1", parent, _ => tcs.Task);

        Assert.True(loader.IsLoading("1"));
        Assert.False(loader.HasError("1"));

        tcs.SetResult(new List<Node>());
        await task;

        Assert.False(loader.IsLoading("1"));
    }

    [Fact]
    public async Task LoadAsyncSetsErrorStateOnFailure()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            loader.LoadAsync("1", parent, _ =>
                Task.FromException<IEnumerable<Node>>(new InvalidOperationException("fail"))));

        Assert.False(loader.IsLoading("1"));
        Assert.True(loader.HasError("1"));
    }

    [Fact]
    public async Task LoadPageAsyncReturnsPageResult()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };

        var result = await loader.LoadPageAsync("1", parent, 0, 5,
            (_, skip, take) => Task.FromResult(new ChildPageResult<Node>
            {
                Items = new List<Node>
                {
                    new() { Id = "1.1", Name = "Child 1" }
                },
                TotalCount = 10
            }));

        Assert.Single(result.Items);
        Assert.Equal(10, result.TotalCount);
    }

    [Fact]
    public void GetCachedChildrenReturnsNullWhenNotCached()
    {
        var loader = new LazyChildLoader<Node>();
        Assert.Null(loader.GetCachedChildren("nonexistent"));
    }

    [Fact]
    public async Task GetCachedChildrenReturnsCachedResult()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };

        await loader.LoadAsync("1", parent, _ =>
            Task.FromResult<IEnumerable<Node>>(new List<Node>
            {
                new() { Id = "1.1", Name = "Child" }
            }));

        var cached = loader.GetCachedChildren("1");
        Assert.NotNull(cached);
        Assert.Single(cached!);
    }

    [Fact]
    public async Task ClearCacheClearsSpecificNode()
    {
        var loader = new LazyChildLoader<Node>();
        var parent = new Node { Id = "1", Name = "Parent" };

        await loader.LoadAsync("1", parent, _ =>
            Task.FromResult<IEnumerable<Node>>(new List<Node>()));

        loader.ClearCache("1");
        Assert.Null(loader.GetCachedChildren("1"));
    }

    [Fact]
    public async Task ClearCacheClearsAll()
    {
        var loader = new LazyChildLoader<Node>();

        await loader.LoadAsync("1", new Node { Id = "1" }, _ =>
            Task.FromResult<IEnumerable<Node>>(new List<Node>()));
        await loader.LoadAsync("2", new Node { Id = "2" }, _ =>
            Task.FromResult<IEnumerable<Node>>(new List<Node>()));

        loader.ClearCache();
        Assert.Null(loader.GetCachedChildren("1"));
        Assert.Null(loader.GetCachedChildren("2"));
    }
}
