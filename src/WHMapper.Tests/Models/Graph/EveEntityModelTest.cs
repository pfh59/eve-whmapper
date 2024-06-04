using Microsoft.AspNetCore.Mvc;
using Pipelines.Sockets.Unofficial.Arenas;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveAPI.Corporation;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using Xunit.Priority;

namespace WHMapper.Tests.Models.Graph
{
    
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]

    public class EveEntityModelTest
    {


        public EveEntityModelTest()
        {
        }



        [Fact]
        public Task CharactereEntity_Model_Test()
        {
            
            var fake_eveapi_charactere = new Character();
            fake_eveapi_charactere.Name="Test Charactere";

            var char_entity = new CharactereEntity(1,fake_eveapi_charactere);
            Assert.NotNull(char_entity);
            Assert.Equal(1,char_entity.Id);
            Assert.Equal("Test Charactere",char_entity.Name);
            Assert.Equal(EveEntityEnums.Character,char_entity.EntityType);
            return Task.CompletedTask;
        }

        [Fact]
        public Task CoorporationEntity_Model_Test()
        {
            var fake_eveapi_corporation = new Corporation();
            fake_eveapi_corporation.Name="Test Corporation";
            
            var coorpo_entity = new CorporationEntity(1,fake_eveapi_corporation);
            Assert.NotNull(coorpo_entity);
            Assert.Equal(1,coorpo_entity.Id);
            Assert.Equal("Test Corporation",coorpo_entity.Name);
            Assert.Equal(EveEntityEnums.Corporation,coorpo_entity.EntityType);

            return Task.CompletedTask;
        }

        [Fact]
        public Task AllianceEntity_Model_Test()
        {
            var fake_eveapi_alliance = new Alliance();
            fake_eveapi_alliance.Name="Test Alliance";

            var alliance_entity = new AllianceEntity(1,fake_eveapi_alliance);
            Assert.NotNull(alliance_entity);
            Assert.Equal(1,alliance_entity.Id);
            Assert.Equal("Test Alliance",alliance_entity.Name);
            Assert.Equal(EveEntityEnums.Alliance,alliance_entity.EntityType);

            return Task.CompletedTask;
        }

/*
        [Fact]
        public Task ShipEntity_Model_Test()
        {
            var fake_eveapi_type = new Type();
            fake_eveapi_type.Name="Test Ship";

            var ship_entity = new ShipEntity(1,fake_eveapi_type);
            Assert.NotNull(ship_entity);
            Assert.Equal(1,ship_entity.Id);
            Assert.Equal("Test Ship",ship_entity.Name);

            ship_entity = new ShipEntity(1,"Test Ship");
            Assert.NotNull(ship_entity);
            Assert.Equal(1,ship_entity.Id);
            Assert.Equal("Test Ship",ship_entity.Name);
            Assert.Equal(EveEntityEnums.Ship,ship_entity.EntityType);

            return Task.CompletedTask;
        }

        [Fact]
        public Task SystemEntity_Model_Test()
        {
            

            var system_entity = new SystemEntity(1,"Test System");
            Assert.NotNull(system_entity);
            Assert.Equal(1,system_entity.Id);
            Assert.Equal("Test System",system_entity.Name);
            Assert.Equal(EveEntityEnums.System,system_entity.EntityType);

            return Task.CompletedTask;
        }*/
    }
}