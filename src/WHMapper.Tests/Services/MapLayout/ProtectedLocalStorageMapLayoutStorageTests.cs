using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using WHMapper.Models.DTO.MapLayout;
using WHMapper.Services.MapLayout;

namespace WHMapper.Tests.Services.MapLayout;

public class ProtectedLocalStorageMapLayoutStorageTests
{
    private const int MAP_ID = 42;

    private readonly FakeLocalStorageJsRuntime _jsRuntime = new();
    private readonly Mock<ILogger<ProtectedLocalStorageMapLayoutStorage>> _loggerMock = new();
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly ProtectedLocalStorageMapLayoutStorage _sut;

    public ProtectedLocalStorageMapLayoutStorageTests()
    {
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _protectedLocalStorage = new ProtectedLocalStorage(_jsRuntime, new EphemeralDataProtectionProvider());
        _sut = new ProtectedLocalStorageMapLayoutStorage(_protectedLocalStorage, _loggerMock.Object);
    }

    private static MapLayoutDto Layout() =>
        new(MapLayoutService.CURRENT_LAYOUT_VERSION,
            [new MapPanelLayoutDto(nameof(MapPanelId.Notes), 10, 20, false, true)]);

    /// <summary>Saves a layout so the test can read the storage key the sut uses.</summary>
    private async Task<string> StoreLayoutAsync()
    {
        await _sut.SetAsync(MAP_ID, Layout());
        return _jsRuntime.Items.Keys.Single();
    }

    [Fact]
    public async Task GetAsync_WhenNothingStored_ReturnsNull()
    {
        Assert.Null(await _sut.GetAsync(MAP_ID));
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_RoundTripsTheLayout()
    {
        await _sut.SetAsync(MAP_ID, Layout());

        var restored = await _sut.GetAsync(MAP_ID);

        Assert.NotNull(restored);
        Assert.Equal(Layout().Version, restored.Version);
        Assert.Equal(Layout().Panels, restored.Panels);
    }

    [Fact]
    public async Task SetAsync_KeepsLayoutsOfDifferentMapsApart()
    {
        await _sut.SetAsync(MAP_ID, Layout());

        Assert.Null(await _sut.GetAsync(MAP_ID + 1));
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("not a layout")]
    public async Task GetAsync_WhenPayloadIsNotALayout_ReturnsNull(string payload)
    {
        var key = await StoreLayoutAsync();
        await _protectedLocalStorage.SetAsync(key, payload);

        Assert.Null(await _sut.GetAsync(MAP_ID));
    }

    [Fact]
    public async Task GetAsync_WhenPayloadCannotBeUnprotected_ReturnsNull()
    {
        var key = await StoreLayoutAsync();
        _jsRuntime.Items[key] = "tampered";

        Assert.Null(await _sut.GetAsync(MAP_ID));
    }

    [Fact]
    public async Task RemoveAsync_DeletesTheStoredLayout()
    {
        await StoreLayoutAsync();

        await _sut.RemoveAsync(MAP_ID);

        Assert.Empty(_jsRuntime.Items);
    }

    [Fact]
    public async Task WhenCircuitIsDisconnected_OperationsDoNotThrow()
    {
        _jsRuntime.IsDisconnected = true;

        Assert.Null(await _sut.GetAsync(MAP_ID));
        await _sut.SetAsync(MAP_ID, Layout());
        await _sut.RemoveAsync(MAP_ID);

        _loggerMock.VerifyLog(LogLevel.Debug, Times.Exactly(3));
    }

    /// <summary>In-memory stand-in for the browser's <c>localStorage</c>.</summary>
    private sealed class FakeLocalStorageJsRuntime : IJSRuntime
    {
        public Dictionary<string, string> Items { get; } = [];

        public bool IsDisconnected { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (IsDisconnected)
            {
                throw new JSDisconnectedException("Circuit disconnected");
            }

            var key = (string)args![0]!;
            object? result = null;
            switch (identifier)
            {
                case "localStorage.getItem":
                    result = Items.GetValueOrDefault(key);
                    break;
                case "localStorage.setItem":
                    Items[key] = (string)args[1]!;
                    break;
                case "localStorage.removeItem":
                    Items.Remove(key);
                    break;
                default:
                    throw new NotSupportedException(identifier);
            }

            return ValueTask.FromResult(result is TValue value ? value : default!);
        }
    }
}
