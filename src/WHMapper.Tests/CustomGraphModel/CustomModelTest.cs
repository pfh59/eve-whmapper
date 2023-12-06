using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.Enums;
using Xunit.Priority;

namespace WHMapper.Tests.CustomGraphModel
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class CustomModelTest
    {
        private const int DEFAULT_MAP_ID = 1;
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const char SOLAR_SYSTEM_EXTENSION_NAME = 'B';
        private const string CONSTELLATION_JITA_NAME = "Kimotoro";
        private const string REGION_JITA_NAME = "The Forge";



        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string CONSTELLATION_WH_NAME = "C-C00113";
        private const string REGION_WH_NAME = "C-R00012";
        private const EveSystemType SOLAR_SYSTEM_WH_CLASS = EveSystemType.C3;
        private const WHEffect SOLAR_SYSTEM_WH_EFFECT = WHEffect.Pulsar;
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";

        private const string USERNAME1 = "FOOBAR1";
        private const string USERNAME2 = "FOOBAR2";


        public CustomModelTest()
        {


        }


        [Fact]
        public async Task Eve_System_Node_Model()
        {
            var node = new EveSystemNodeModel(new Models.Db.WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, SOLAR_SYSTEM_EXTENSION_NAME, 1.0F), new Models.Db.WHNote(SOLAR_SYSTEM_JITA_ID,WHSystemStatusEnum.Friendly,SOLAR_SYSTEM_JITA_NAME), REGION_JITA_NAME, CONSTELLATION_JITA_NAME);
            Assert.NotNull(node);
            Assert.Equal(0, node.IdWH);
            Assert.Equal(DEFAULT_MAP_ID, node.IdWHMap);
            Assert.Equal(SOLAR_SYSTEM_JITA_ID, node.SolarSystemId);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, node.Title);
            Assert.Equal(EveSystemType.HS, node.SystemType);
            Assert.Equal("B", node.NameExtension);
            Assert.Empty(node.ConnectedUsers);
            Assert.False(node.Locked);
            Assert.Equal(WHSystemStatusEnum.Friendly,node.SystemStatus);

            await node.AddConnectedUser(USERNAME1);
            await node.AddConnectedUser(USERNAME2);
            Assert.Contains(USERNAME1, node.ConnectedUsers);
            Assert.Contains(USERNAME2, node.ConnectedUsers);

            await node.RemoveConnectedUser(USERNAME2);
            Assert.Contains(USERNAME1, node.ConnectedUsers);
            Assert.DoesNotContain(USERNAME2, node.ConnectedUsers);
        }

        [Fact]
        public void Eve_System_Link_Model()
        {
            var node = new EveSystemNodeModel(new Models.Db.WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, 1.0F), null, REGION_JITA_NAME, CONSTELLATION_JITA_NAME);
             Assert.Equal(WHSystemStatusEnum.Unknown,node.SystemStatus);
            var node2 = new EveSystemNodeModel(new Models.Db.WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, -1.0F), null, REGION_WH_NAME, CONSTELLATION_WH_NAME,SOLAR_SYSTEM_WH_CLASS, SOLAR_SYSTEM_WH_EFFECT,null, new List<WHStatic>() { new WHStatic(SOLAR_SYSTEM_WH_STATICS,EveSystemType.C3) }) ;


            var link = new EveSystemLinkModel(new Models.Db.WHSystemLink(1, 2), node, node2);
            Assert.NotNull(link);
            var srcEveNodeModel = link.Source.Model as EveSystemNodeModel;
            var targetEveNodeModel = link.Target.Model as EveSystemNodeModel;
            Assert.NotNull(srcEveNodeModel);
            Assert.NotNull(targetEveNodeModel);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, srcEveNodeModel.Name);
            Assert.Equal(SOLAR_SYSTEM_WH_NAME, targetEveNodeModel.Name);
            Assert.False(link.IsEoL);
            Assert.Equal(SystemLinkMassStatus.Normal, link.MassStatus);
            Assert.Equal(SystemLinkSize.Large, link.Size);
           

            link.Size= SystemLinkSize.Small;
            link.MassStatus=SystemLinkMassStatus.Normal;
            Assert.Equal(SystemLinkSize.Small, link.Size);
            Assert.Equal(SystemLinkMassStatus.Normal, link.MassStatus);
            Assert.NotEmpty(link.Labels);


        }
    }
}

