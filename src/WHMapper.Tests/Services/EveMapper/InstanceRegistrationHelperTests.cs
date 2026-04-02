using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;

namespace WHMapper.Tests.Services.EveMapper;

public class InstanceRegistrationHelperTests
{
    #region LoadRegistrationContextAsync

    [Theory]
    [InlineAutoMoqData(null!)]
    [InlineAutoMoqData("")]
    public async Task LoadRegistrationContext_WhenClientIdIsNullOrEmpty_ReturnsUnauthenticatedContext(
        string? clientId,
        InstanceRegistrationHelper sut)
    {
        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.NotNull(result);
        Assert.False(result.IsAuthenticated);
        Assert.Equal(0, result.CharacterId);
    }

    [Theory]
    [AutoMoqData]
    public async Task LoadRegistrationContext_WhenPrimaryAccountIsNull_ReturnsUnauthenticatedContext(
        [Frozen] Mock<IEveMapperUserManagementService> userManagement,
        string clientId,
        InstanceRegistrationHelper sut)
    {
        userManagement.Setup(x => x.GetPrimaryAccountAsync(clientId))
            .ReturnsAsync((WHMapperUser?)null);

        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.NotNull(result);
        Assert.False(result.IsAuthenticated);
    }

    [Theory]
    [AutoMoqData]
    public async Task LoadRegistrationContext_WhenCharacterLoadFails_ReturnsAuthenticatedWithoutCharacterInfo(
        [Frozen] Mock<IEveMapperUserManagementService> userManagement,
        [Frozen] Mock<ICharacterServices> characterServices,
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        string clientId,
        InstanceRegistrationHelper sut)
    {
        var user = new WHMapperUser(123, "http://portrait.url");
        userManagement.Setup(x => x.GetPrimaryAccountAsync(clientId))
            .ReturnsAsync(user);

        characterServices.Setup(x => x.GetCharacter(123))
            .ReturnsAsync(Result<Character>.Failure("Not found"));

        instanceService.Setup(x => x.GetAdministeredInstancesAsync(123))
            .ReturnsAsync((IEnumerable<WHInstance>?)null);

        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.NotNull(result);
        Assert.True(result.IsAuthenticated);
        Assert.Equal(123, result.CharacterId);
        Assert.Null(result.CharacterInfo);
        Assert.Equal(string.Empty, result.CharacterName);
    }

    [Theory]
    [AutoMoqData]
    public async Task LoadRegistrationContext_WhenFullDataAvailable_ReturnsCompleteContext(
        [Frozen] Mock<IEveMapperUserManagementService> userManagement,
        [Frozen] Mock<ICharacterServices> characterServices,
        [Frozen] Mock<IEveMapperService> eveMapperService,
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        string clientId,
        InstanceRegistrationHelper sut)
    {
        var user = new WHMapperUser(42, "http://portrait.url");
        userManagement.Setup(x => x.GetPrimaryAccountAsync(clientId))
            .ReturnsAsync(user);

        var character = new Character
        {
            Name = "Test Pilot",
            CorporationId = 100,
            AllianceId = 200
        };
        characterServices.Setup(x => x.GetCharacter(42))
            .ReturnsAsync(Result<Character>.Success(character));

        var corpEntity = new CorporationEntity(100, "Test Corp");
        eveMapperService.Setup(x => x.GetCorporation(100))
            .ReturnsAsync(corpEntity);

        var allianceEntity = new AllianceEntity(200, "Test Alliance");
        eveMapperService.Setup(x => x.GetAlliance(200))
            .ReturnsAsync(allianceEntity);

        instanceService.Setup(x => x.GetAdministeredInstancesAsync(42))
            .ReturnsAsync((IEnumerable<WHInstance>?)null);

        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.NotNull(result);
        Assert.True(result.IsAuthenticated);
        Assert.Equal(42, result.CharacterId);
        Assert.Equal("Test Pilot", result.CharacterName);
        Assert.Equal("Test Corp", result.CorporationName);
        Assert.Equal("Test Alliance", result.AllianceName);
        Assert.False(result.AlreadyHasInstance);
    }

    [Theory]
    [AutoMoqData]
    public async Task LoadRegistrationContext_WhenCharacterHasNoCorp_SkipsCorporationLookup(
        [Frozen] Mock<IEveMapperUserManagementService> userManagement,
        [Frozen] Mock<ICharacterServices> characterServices,
        [Frozen] Mock<IEveMapperService> eveMapperService,
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        string clientId,
        InstanceRegistrationHelper sut)
    {
        var user = new WHMapperUser(42, "http://portrait.url");
        userManagement.Setup(x => x.GetPrimaryAccountAsync(clientId))
            .ReturnsAsync(user);

        var character = new Character
        {
            Name = "Solo Pilot",
            CorporationId = 0,
            AllianceId = 0
        };
        characterServices.Setup(x => x.GetCharacter(42))
            .ReturnsAsync(Result<Character>.Success(character));

        instanceService.Setup(x => x.GetAdministeredInstancesAsync(42))
            .ReturnsAsync((IEnumerable<WHInstance>?)null);

        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.Equal(string.Empty, result.CorporationName);
        Assert.Equal(string.Empty, result.AllianceName);
        eveMapperService.Verify(x => x.GetCorporation(It.IsAny<int>()), Times.Never);
        eveMapperService.Verify(x => x.GetAlliance(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [AutoMoqData]
    public async Task LoadRegistrationContext_WhenExistingInstanceFound_SetsAlreadyHasInstance(
        [Frozen] Mock<IEveMapperUserManagementService> userManagement,
        [Frozen] Mock<ICharacterServices> characterServices,
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        string clientId,
        InstanceRegistrationHelper sut)
    {
        var user = new WHMapperUser(42, "http://portrait.url");
        userManagement.Setup(x => x.GetPrimaryAccountAsync(clientId))
            .ReturnsAsync(user);

        characterServices.Setup(x => x.GetCharacter(42))
            .ReturnsAsync(Result<Character>.Failure("Not found"));

        var existingInstance = new WHInstance("Existing", 42, "Test", WHAccessEntity.Character, 42, "Test");
        // Set Id via reflection since it's a DB entity
        typeof(WHInstance).GetProperty("Id")!.SetValue(existingInstance, 99);

        instanceService.Setup(x => x.GetAdministeredInstancesAsync(42))
            .ReturnsAsync(new[] { existingInstance });

        var result = await sut.LoadRegistrationContextAsync(clientId);

        Assert.True(result.AlreadyHasInstance);
        Assert.Equal(99, result.ExistingInstanceId);
    }

    #endregion

    #region RegisterInstanceAsync

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WithCharacterOwner_CreatesInstanceForCharacter(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot",
            CharacterInfo = new Character { CorporationId = 100, AllianceId = 200 }
        };

        var expectedInstance = new WHInstance("My Instance", 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot");
        instanceService.Setup(x => x.CanRegisterAsync(42)).ReturnsAsync(true);
        instanceService.Setup(x => x.CreateInstanceAsync(
            "My Instance", null, 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"))
            .ReturnsAsync(expectedInstance);

        var result = await sut.RegisterInstanceAsync(context, "My Instance", null, WHAccessEntity.Character);

        Assert.NotNull(result);
        instanceService.Verify(x => x.CreateInstanceAsync(
            "My Instance", null, 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WithCorporationOwner_CreatesInstanceForCorporation(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot",
            CorporationName = "Test Corp",
            CharacterInfo = new Character { CorporationId = 100, AllianceId = 200 }
        };

        var expectedInstance = new WHInstance("Corp Instance", 100, "Test Corp", WHAccessEntity.Corporation, 42, "Test Pilot");
        instanceService.Setup(x => x.CanRegisterAsync(100)).ReturnsAsync(true);
        instanceService.Setup(x => x.CreateInstanceAsync(
            "Corp Instance", null, 100, "Test Corp", WHAccessEntity.Corporation, 42, "Test Pilot"))
            .ReturnsAsync(expectedInstance);

        var result = await sut.RegisterInstanceAsync(context, "Corp Instance", null, WHAccessEntity.Corporation);

        Assert.NotNull(result);
        instanceService.Verify(x => x.CreateInstanceAsync(
            "Corp Instance", null, 100, "Test Corp", WHAccessEntity.Corporation, 42, "Test Pilot"), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WithAllianceOwner_CreatesInstanceForAlliance(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot",
            AllianceName = "Test Alliance",
            CharacterInfo = new Character { CorporationId = 100, AllianceId = 200 }
        };

        var expectedInstance = new WHInstance("Alliance Instance", 200, "Test Alliance", WHAccessEntity.Alliance, 42, "Test Pilot");
        instanceService.Setup(x => x.CanRegisterAsync(200)).ReturnsAsync(true);
        instanceService.Setup(x => x.CreateInstanceAsync(
            "Alliance Instance", null, 200, "Test Alliance", WHAccessEntity.Alliance, 42, "Test Pilot"))
            .ReturnsAsync(expectedInstance);

        var result = await sut.RegisterInstanceAsync(context, "Alliance Instance", null, WHAccessEntity.Alliance);

        Assert.NotNull(result);
        instanceService.Verify(x => x.CreateInstanceAsync(
            "Alliance Instance", null, 200, "Test Alliance", WHAccessEntity.Alliance, 42, "Test Pilot"), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WithWhitespaceDescription_PassesNullDescription(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot"
        };

        instanceService.Setup(x => x.CanRegisterAsync(42)).ReturnsAsync(true);
        instanceService.Setup(x => x.CreateInstanceAsync(
            "Test", null, 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"))
            .ReturnsAsync(new WHInstance("Test", 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"));

        await sut.RegisterInstanceAsync(context, "Test", "   ", WHAccessEntity.Character);

        instanceService.Verify(x => x.CreateInstanceAsync(
            "Test", null, 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WithValidDescription_PassesDescription(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot"
        };

        instanceService.Setup(x => x.CanRegisterAsync(42)).ReturnsAsync(true);
        instanceService.Setup(x => x.CreateInstanceAsync(
            "Test", "A description", 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"))
            .ReturnsAsync(new WHInstance("Test", 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"));

        await sut.RegisterInstanceAsync(context, "Test", "A description", WHAccessEntity.Character);

        instanceService.Verify(x => x.CreateInstanceAsync(
            "Test", "A description", 42, "Test Pilot", WHAccessEntity.Character, 42, "Test Pilot"), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WhenCorporationInfoMissing_ThrowsInvalidOperationException(
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot",
            CharacterInfo = null
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.RegisterInstanceAsync(context, "Test", null, WHAccessEntity.Corporation));

        Assert.Equal("Corporation information not available", ex.Message);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WhenAllianceInfoMissing_ThrowsInvalidOperationException(
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot",
            CharacterInfo = new Character { AllianceId = 0 }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.RegisterInstanceAsync(context, "Test", null, WHAccessEntity.Alliance));

        Assert.Equal("Alliance information not available", ex.Message);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WhenEntityAlreadyRegistered_ThrowsInvalidOperationException(
        [Frozen] Mock<IEveMapperInstanceService> instanceService,
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot"
        };

        instanceService.Setup(x => x.CanRegisterAsync(42)).ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.RegisterInstanceAsync(context, "Test", null, WHAccessEntity.Character));

        Assert.Equal("An instance already exists for this entity", ex.Message);
    }

    [Theory]
    [AutoMoqData]
    public async Task RegisterInstance_WhenInvalidOwnerType_ThrowsInvalidOperationException(
        InstanceRegistrationHelper sut)
    {
        var context = new InstanceRegistrationContext
        {
            CharacterId = 42,
            CharacterName = "Test Pilot"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.RegisterInstanceAsync(context, "Test", null, (WHAccessEntity)999));

        Assert.Equal("Invalid owner type selected", ex.Message);
    }

    #endregion
}
