using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHUserSettings;
using WHMapper.Services.WHUserSettings;

namespace WHMapper.Tests.Services.WHUserSettings;

public class WHUserSettingServiceTests
{
    private const int CHARACTER_ID = 2113720458;

    [Theory, AutoMoqData]
    public async Task GetSettingsAsync_WhenSettingsExist_ReturnsExistingSettings(
        WHUserSetting existingSettings,
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.GetByCharacterId(existingSettings.EveCharacterId))
                .ReturnsAsync(existingSettings);

        var result = await sut.GetSettingsAsync(existingSettings.EveCharacterId);

        Assert.Equal(existingSettings, result);
    }

    [Theory, AutoMoqData]
    public async Task GetSettingsAsync_WhenNoSettings_ReturnsDefaults(
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.GetByCharacterId(CHARACTER_ID))
                .ReturnsAsync((WHUserSetting?)null);

        var result = await sut.GetSettingsAsync(CHARACTER_ID);

        Assert.NotNull(result);
        Assert.Equal(CHARACTER_ID, result.EveCharacterId);
        Assert.Equal(WHUserSetting.DEFAULT_KEY_LINK, result.KeyLink);
        Assert.Equal(WHUserSetting.DEFAULT_KEY_DELETE, result.KeyDelete);
        Assert.Equal(WHUserSetting.DEFAULT_ZOOM_ENABLED, result.ZoomEnabled);
        Assert.Equal(WHUserSetting.DEFAULT_NODE_SPACING, result.NodeSpacing);
        Assert.Equal(WHUserSetting.DEFAULT_DRAG_THRESHOLD, result.DragThreshold);
    }

    [Theory, AutoMoqData]
    public async Task SaveSettingsAsync_WhenExistingRecord_UpdatesAndFiresEvent(
        WHUserSetting incoming,
        WHUserSetting existing,
        WHUserSetting updated,
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.GetByCharacterId(incoming.EveCharacterId))
                .ReturnsAsync(existing);
        repoMock.Setup(r => r.Update(existing.Id, incoming))
                .ReturnsAsync(updated);

        WHUserSetting? eventArgs = null;
        sut.OnSettingsChanged += s => { eventArgs = s; return Task.CompletedTask; };

        var result = await sut.SaveSettingsAsync(incoming);

        Assert.Equal(updated, result);
        Assert.Equal(updated, eventArgs);
        repoMock.Verify(r => r.Update(existing.Id, incoming), Times.Once);
        repoMock.Verify(r => r.Create(It.IsAny<WHUserSetting>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task SaveSettingsAsync_WhenNoExistingRecord_CreatesAndFiresEvent(
        WHUserSetting incoming,
        WHUserSetting created,
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.GetByCharacterId(incoming.EveCharacterId))
                .ReturnsAsync((WHUserSetting?)null);
        repoMock.Setup(r => r.Create(incoming))
                .ReturnsAsync(created);

        WHUserSetting? eventArgs = null;
        sut.OnSettingsChanged += s => { eventArgs = s; return Task.CompletedTask; };

        var result = await sut.SaveSettingsAsync(incoming);

        Assert.Equal(created, result);
        Assert.Equal(created, eventArgs);
        repoMock.Verify(r => r.Create(incoming), Times.Once);
        repoMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<WHUserSetting>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task SaveSettingsAsync_WhenSaveFails_ReturnsNullAndNoEvent(
        WHUserSetting incoming,
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.GetByCharacterId(incoming.EveCharacterId))
                .ReturnsAsync((WHUserSetting?)null);
        repoMock.Setup(r => r.Create(incoming))
                .ReturnsAsync((WHUserSetting?)null);

        bool eventFired = false;
        sut.OnSettingsChanged += _ => { eventFired = true; return Task.CompletedTask; };

        var result = await sut.SaveSettingsAsync(incoming);

        Assert.Null(result);
        Assert.False(eventFired);
    }

    [Theory, AutoMoqData]
    public async Task ResetToDefaultsAsync_WhenDeleted_ReturnsTrueAndFiresEventWithDefaults(
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.DeleteByCharacterId(CHARACTER_ID))
                .ReturnsAsync(true);

        WHUserSetting? eventArgs = null;
        sut.OnSettingsChanged += s => { eventArgs = s; return Task.CompletedTask; };

        var result = await sut.ResetToDefaultsAsync(CHARACTER_ID);

        Assert.True(result);
        Assert.NotNull(eventArgs);
        Assert.Equal(CHARACTER_ID, eventArgs.EveCharacterId);
        Assert.Equal(WHUserSetting.DEFAULT_KEY_LINK, eventArgs.KeyLink);
        Assert.Equal(WHUserSetting.DEFAULT_ZOOM_ENABLED, eventArgs.ZoomEnabled);
        Assert.Equal(WHUserSetting.DEFAULT_NODE_SPACING, eventArgs.NodeSpacing);
    }

    [Theory, AutoMoqData]
    public async Task ResetToDefaultsAsync_WhenNotDeleted_ReturnsFalseAndNoEvent(
        [Frozen] Mock<IWHUserSettingRepository> repoMock,
        WHUserSettingService sut)
    {
        repoMock.Setup(r => r.DeleteByCharacterId(CHARACTER_ID))
                .ReturnsAsync(false);

        bool eventFired = false;
        sut.OnSettingsChanged += _ => { eventFired = true; return Task.CompletedTask; };

        var result = await sut.ResetToDefaultsAsync(CHARACTER_ID);

        Assert.False(result);
        Assert.False(eventFired);
    }
}
