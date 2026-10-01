using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.DTO.MapLayout;
using WHMapper.Services.MapLayout;

namespace WHMapper.Tests.Services.MapLayout;

public class MapLayoutServiceTests
{
    private const int MAP_ID = 42;
    private const double CONTAINER_WIDTH = 1600;
    private const double CONTAINER_HEIGHT = 900;

    private static MapPanelLayout Panel(IReadOnlyList<MapPanelLayout> panels, MapPanelId id) =>
        panels.Single(p => p.Id == id);

    private static IReadOnlyList<MapPanelLayout> Defaults(MapLayoutService sut) =>
        sut.CreateDefaultLayout(CONTAINER_WIDTH, CONTAINER_HEIGHT);

    private static IReadOnlyList<MapPanelLayout> Merge(MapLayoutService sut, params MapPanelLayoutDto[] panels) =>
        sut.Merge(new MapLayoutDto(MapLayoutService.CURRENT_LAYOUT_VERSION, panels), CONTAINER_WIDTH, CONTAINER_HEIGHT);

    [Theory, AutoMoqData]
    public void CreateDefaultLayout_ReturnsOneVisibleExpandedPanelPerId(MapLayoutService sut)
    {
        var panels = Defaults(sut);

        Assert.Equal(Enum.GetValues<MapPanelId>().OrderBy(x => x), panels.Select(p => p.Id).OrderBy(x => x));
        Assert.All(panels, panel =>
        {
            Assert.True(panel.IsUserVisible);
            Assert.False(panel.IsCollapsed);
        });
    }

    [Theory, AutoMoqData]
    public void CreateDefaultLayout_PlacesEveryPanelInsideTheMap(MapLayoutService sut)
    {
        Assert.All(Defaults(sut), panel =>
        {
            Assert.InRange(panel.X, 0, CONTAINER_WIDTH - MapPanelDefaults.GetWidth(panel.Id));
            Assert.InRange(panel.Y, 0, CONTAINER_HEIGHT - MapPanelDefaults.TITLE_BAR_HEIGHT);
        });
    }

    [Theory, AutoMoqData]
    public void CreateDefaultLayout_AssignsDistinctZOrders(MapLayoutService sut)
    {
        var panels = Defaults(sut);

        Assert.Equal(panels.Count, panels.Select(p => p.ZOrder).Distinct().Count());
    }

    [Theory, AutoMoqData]
    public void MoveBy_AppliesOffset(MapLayoutService sut)
    {
        var panels = Defaults(sut);
        var notes = Panel(panels, MapPanelId.Notes);
        var (x, y) = (notes.X, notes.Y);

        sut.MoveBy(panels, MapPanelId.Notes, -30, 40, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(x - 30, notes.X);
        Assert.Equal(y + 40, notes.Y);
    }

    [Theory, AutoMoqData]
    public void MoveBy_AffectsOnlyTheTargetedPanel(MapLayoutService sut)
    {
        var panels = Defaults(sut);
        var untouched = Panel(panels, MapPanelId.Signatures);
        var (x, y) = (untouched.X, untouched.Y);

        sut.MoveBy(panels, MapPanelId.Notes, -30, 40, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(x, untouched.X);
        Assert.Equal(y, untouched.Y);
    }

    [Theory, AutoMoqData]
    public void MoveBy_KeepsPanelGrabbableWhenDraggedOffTheRight(MapLayoutService sut)
    {
        var result = sut.MoveBy(Defaults(sut), MapPanelId.Notes, 99_999, 0, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(CONTAINER_WIDTH - MapPanelDefaults.MIN_VISIBLE_WIDTH, Panel(result, MapPanelId.Notes).X);
    }

    [Theory, AutoMoqData]
    public void MoveBy_KeepsPanelGrabbableWhenDraggedOffTheLeft(MapLayoutService sut)
    {
        var result = sut.MoveBy(Defaults(sut), MapPanelId.Notes, -99_999, 0, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(
            MapPanelDefaults.MIN_VISIBLE_WIDTH - MapPanelDefaults.GetWidth(MapPanelId.Notes),
            Panel(result, MapPanelId.Notes).X);
    }

    [Theory, AutoMoqData]
    public void MoveBy_KeepsTitleBarWithinVerticalBounds(MapLayoutService sut)
    {
        var panels = Defaults(sut);

        Assert.Equal(0, Panel(sut.MoveBy(panels, MapPanelId.Notes, 0, -99_999, CONTAINER_WIDTH, CONTAINER_HEIGHT), MapPanelId.Notes).Y);
        Assert.Equal(
            CONTAINER_HEIGHT - MapPanelDefaults.TITLE_BAR_HEIGHT,
            Panel(sut.MoveBy(panels, MapPanelId.Notes, 0, 99_999, CONTAINER_WIDTH, CONTAINER_HEIGHT), MapPanelId.Notes).Y);
    }

    [Theory, AutoMoqData]
    public void MoveBy_IgnoresNonFiniteOffsets(MapLayoutService sut)
    {
        var panels = Defaults(sut);
        var notes = Panel(panels, MapPanelId.Notes);
        var (x, y) = (notes.X, notes.Y);

        sut.MoveBy(panels, MapPanelId.Notes, double.NaN, double.PositiveInfinity, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(x, notes.X);
        Assert.Equal(y, notes.Y);
    }

    [Theory, AutoMoqData]
    public void MoveBy_FallsBackWhenContainerSizeIsUnknown(MapLayoutService sut)
    {
        var result = sut.MoveBy(Defaults(sut), MapPanelId.Notes, 99_999, 0, 0, 0);

        Assert.Equal(
            MapPanelDefaults.FALLBACK_CONTAINER_WIDTH - MapPanelDefaults.MIN_VISIBLE_WIDTH,
            Panel(result, MapPanelId.Notes).X);
    }

    [Theory, AutoMoqData]
    public void Merge_WhenNothingStored_ReturnsDefaults(MapLayoutService sut)
    {
        var merged = sut.Merge(null, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(Defaults(sut).Select(p => (p.Id, p.X, p.Y)), merged.Select(p => (p.Id, p.X, p.Y)));
    }

    [Theory, AutoMoqData]
    public void Merge_WhenVersionIsUnsupported_ReturnsDefaults(MapLayoutService sut)
    {
        var stored = new MapLayoutDto(
            MapLayoutService.CURRENT_LAYOUT_VERSION + 1,
            [new MapPanelLayoutDto(nameof(MapPanelId.Notes), 11, 22, false, true)]);

        var notes = Panel(sut.Merge(stored, CONTAINER_WIDTH, CONTAINER_HEIGHT), MapPanelId.Notes);

        Assert.True(notes.IsUserVisible);
        Assert.NotEqual(11, notes.X);
    }

    [Theory, AutoMoqData]
    public void Merge_AppliesStoredPositionAndState(MapLayoutService sut)
    {
        var signatures = Panel(
            Merge(sut, new MapPanelLayoutDto(nameof(MapPanelId.Signatures), 120, 240, false, true)),
            MapPanelId.Signatures);

        Assert.Equal(120, signatures.X);
        Assert.Equal(240, signatures.Y);
        Assert.False(signatures.IsUserVisible);
        Assert.True(signatures.IsCollapsed);
    }

    [Theory, AutoMoqData]
    public void Merge_ClampsStoredPositionIntoASmallerMap(MapLayoutService sut)
    {
        var notes = Panel(
            Merge(sut, new MapPanelLayoutDto(nameof(MapPanelId.Notes), 5_000, 5_000, true, false)),
            MapPanelId.Notes);

        Assert.Equal(CONTAINER_WIDTH - MapPanelDefaults.MIN_VISIBLE_WIDTH, notes.X);
        Assert.Equal(CONTAINER_HEIGHT - MapPanelDefaults.TITLE_BAR_HEIGHT, notes.Y);
    }

    [Theory, AutoMoqData]
    public void Merge_WhenStoredPositionIsNonFinite_KeepsDefaultPositionButAppliesState(MapLayoutService sut)
    {
        var notes = Panel(
            Merge(sut, new MapPanelLayoutDto(nameof(MapPanelId.Notes), double.NaN, 10, false, false)),
            MapPanelId.Notes);
        var expected = Panel(Defaults(sut), MapPanelId.Notes);

        Assert.Equal((expected.X, expected.Y), (notes.X, notes.Y));
        Assert.False(notes.IsUserVisible);
    }

    [Theory, AutoMoqData]
    public void Merge_IgnoresUnknownPanelId(MapLayoutService sut)
    {
        var merged = Merge(sut, new MapPanelLayoutDto("SomeRemovedPanel", 0, 0, false, false));

        Assert.Equal(Enum.GetValues<MapPanelId>().Length, merged.Count);
        Assert.All(merged, panel => Assert.True(panel.IsUserVisible));
    }

    [Theory, AutoMoqData]
    public void Merge_WhenPanelMissingFromStorage_KeepsItsDefaults(MapLayoutService sut)
    {
        var routes = Panel(
            Merge(sut, new MapPanelLayoutDto(nameof(MapPanelId.Notes), 10, 10, false, false)),
            MapPanelId.RoutePlanner);
        var expected = Panel(Defaults(sut), MapPanelId.RoutePlanner);

        Assert.Equal((expected.X, expected.Y), (routes.X, routes.Y));
        Assert.True(routes.IsUserVisible);
    }

    [Theory, AutoMoqData]
    public void BringToFront_RaisesPanelAboveEveryOther(MapLayoutService sut)
    {
        var result = sut.BringToFront(Defaults(sut), MapPanelId.Notes);

        var notes = Panel(result, MapPanelId.Notes);
        Assert.All(result.Where(p => p.Id != MapPanelId.Notes), other => Assert.True(notes.ZOrder > other.ZOrder));
    }

    [Theory, AutoMoqData]
    public void BringToFront_WhenCeilingReached_CompactsBelowTheMaximum(MapLayoutService sut)
    {
        var panels = Defaults(sut);
        Panel(panels, MapPanelId.Signatures).ZOrder = MapLayoutService.MAX_PANEL_Z_ORDER;

        var result = sut.BringToFront(panels, MapPanelId.Notes);

        Assert.All(result, panel => Assert.True(panel.ZOrder <= MapLayoutService.MAX_PANEL_Z_ORDER));
        var notes = Panel(result, MapPanelId.Notes);
        Assert.All(result.Where(p => p.Id != MapPanelId.Notes), other => Assert.True(notes.ZOrder > other.ZOrder));
    }

    [Theory, AutoMoqData]
    public void BringToFront_WhenPanelIsAlreadyOnTop_KeepsZOrders(MapLayoutService sut)
    {
        var panels = sut.BringToFront(Defaults(sut), MapPanelId.Notes);
        var zOrders = panels.Select(p => p.ZOrder).ToList();

        var result = sut.BringToFront(panels, MapPanelId.Notes);

        Assert.Equal(zOrders, result.Select(p => p.ZOrder));
    }

    [Theory, AutoMoqData]
    public void BringToFront_WhenPanelIsMissing_KeepsZOrders(MapLayoutService sut)
    {
        var panels = Defaults(sut).Where(p => p.Id != MapPanelId.Notes).ToList();
        var zOrders = panels.Select(p => p.ZOrder).ToList();

        var result = sut.BringToFront(panels, MapPanelId.Notes);

        Assert.Equal(zOrders, result.Select(p => p.ZOrder));
    }

    [Theory, AutoMoqData]
    public void Close_HidesOnlyTheTargetedPanel(MapLayoutService sut)
    {
        var result = sut.Close(Defaults(sut), MapPanelId.Notes);

        Assert.False(Panel(result, MapPanelId.Notes).IsUserVisible);
        Assert.All(result.Where(p => p.Id != MapPanelId.Notes), panel => Assert.True(panel.IsUserVisible));
    }

    [Theory, AutoMoqData]
    public void Close_IsIdempotent(MapLayoutService sut)
    {
        var result = sut.Close(sut.Close(Defaults(sut), MapPanelId.Notes), MapPanelId.Notes);

        Assert.False(Panel(result, MapPanelId.Notes).IsUserVisible);
    }

    [Theory, AutoMoqData]
    public void ToggleVisibility_AffectsOnlyTheTargetedPanel(MapLayoutService sut)
    {
        var result = sut.ToggleVisibility(Defaults(sut), MapPanelId.Notes);

        Assert.False(Panel(result, MapPanelId.Notes).IsUserVisible);
        Assert.All(result.Where(p => p.Id != MapPanelId.Notes), panel => Assert.True(panel.IsUserVisible));
    }

    [Theory, AutoMoqData]
    public void ToggleCollapse_AffectsOnlyTheTargetedPanel(MapLayoutService sut)
    {
        var result = sut.ToggleCollapse(Defaults(sut), MapPanelId.Signatures);

        Assert.True(Panel(result, MapPanelId.Signatures).IsCollapsed);
        Assert.All(result.Where(p => p.Id != MapPanelId.Signatures), panel => Assert.False(panel.IsCollapsed));
    }

    [Theory, AutoMoqData]
    public async Task LoadAsync_WhenStorageIsEmpty_ReturnsDefaults(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        storageMock.Setup(s => s.GetAsync(MAP_ID)).ReturnsAsync((MapLayoutDto?)null);

        var panels = await sut.LoadAsync(MAP_ID, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        Assert.Equal(Enum.GetValues<MapPanelId>().Length, panels.Count);
    }

    [Theory, AutoMoqData]
    public async Task ScheduleSaveAsync_DebouncesRapidChangesIntoASingleWrite(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        var panels = Defaults(sut);
        var expectedX = Panel(panels, MapPanelId.Notes).X - 50;

        await sut.ScheduleSaveAsync(MAP_ID, panels);
        await sut.ScheduleSaveAsync(MAP_ID, panels);
        await sut.ScheduleSaveAsync(MAP_ID, sut.MoveBy(panels, MapPanelId.Notes, -50, 0, CONTAINER_WIDTH, CONTAINER_HEIGHT));
        await sut.FlushAsync(MAP_ID);

        storageMock.Verify(s => s.SetAsync(MAP_ID, It.IsAny<MapLayoutDto>()), Times.Once);
        storageMock.Verify(
            s => s.SetAsync(MAP_ID, It.Is<MapLayoutDto>(dto =>
                dto.Panels.Single(p => p.PanelId == nameof(MapPanelId.Notes)).X == expectedX)),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task ScheduleSaveAsync_WritesTheCurrentSchemaVersion(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        await sut.ScheduleSaveAsync(MAP_ID, Defaults(sut));
        await sut.FlushAsync(MAP_ID);

        storageMock.Verify(
            s => s.SetAsync(MAP_ID, It.Is<MapLayoutDto>(dto => dto.Version == MapLayoutService.CURRENT_LAYOUT_VERSION)),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task FlushAsync_WhenNothingPending_DoesNotWrite(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        await sut.FlushAsync(MAP_ID);

        storageMock.Verify(s => s.SetAsync(It.IsAny<int>(), It.IsAny<MapLayoutDto>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task ResetAsync_RemovesStorageAndReturnsDefaults(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        var panels = await sut.ResetAsync(MAP_ID, CONTAINER_WIDTH, CONTAINER_HEIGHT);

        storageMock.Verify(s => s.RemoveAsync(MAP_ID), Times.Once);
        Assert.All(panels, panel => Assert.True(panel.IsUserVisible));
    }

    [Theory, AutoMoqData]
    public async Task ResetAsync_CancelsAPendingSave(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        await sut.ScheduleSaveAsync(MAP_ID, Defaults(sut));
        await sut.ResetAsync(MAP_ID, CONTAINER_WIDTH, CONTAINER_HEIGHT);
        await sut.FlushAsync(MAP_ID);

        storageMock.Verify(s => s.SetAsync(It.IsAny<int>(), It.IsAny<MapLayoutDto>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task DisposeAsync_CancelsPendingSaves(
        [Frozen] Mock<IMapLayoutStorage> storageMock,
        MapLayoutService sut)
    {
        await sut.ScheduleSaveAsync(MAP_ID, Defaults(sut));
        await sut.ScheduleSaveAsync(MAP_ID + 1, Defaults(sut));

        await sut.DisposeAsync();
        // Outlast the debounce so a save that survived the dispose would have been written.
        await Task.Delay(MapLayoutService.LAYOUT_SAVE_DEBOUNCE_MS * 2);

        storageMock.Verify(s => s.SetAsync(It.IsAny<int>(), It.IsAny<MapLayoutDto>()), Times.Never);
    }
}
