using Blazor.Diagrams.Core.Models;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.Enums;
using Xunit.Priority;

namespace WHMapper.Tests.Models.Graph
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class GraphModelsTest
    {
        private const int DEFAULT_MAP_ID = 1;
        private const int SOLAR_SYSTEM_JITA_ID = 30000142;
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
        private const char SOLAR_SYSTEM_EXTENSION_NAME = 'B';
        private const string CONSTELLATION_JITA_NAME = "Kimotoro";
        private const string REGION_JITA_NAME = "The Forge";

        private const string SOLAR_SYSTEM_AMAMAKE_NAME = "Amamake";
        private const int SOLAR_SYSTEM_AMAMAKE_ID = 30002537;
        private const string CONSTELLATION_AMAMAKE_NAME = "Hed";
        private const string REGION_AMAMAKE_NAME = "Heimatar";


        private const string SOLAR_SYSTEM_6_CZ49_NAME = "6-CZ49";
        private const int SOLAR_SYSTEM_6_CZ49_ID = 30001161;
        private const float SOLAR_SYSTEM_6_CZ49_SECURITY = -0.16F;
        private const string CONSTELLATION_6_CZ49_NAME = "2-M6DE";
        private const string REGION_6_CZ49_NAME = "Syndicate";


        private const float SOLAR_SYSTEM_JITA_SECURITY = 1.0F;
        private const float SOLAR_SYSTEM_AMAMAKE_SECURITY = -0.4F;


        private const int SOLAR_SYSTEM_WH_ID = 31001123;
        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string CONSTELLATION_WH_NAME = "C-C00113";
        private const string REGION_WH_NAME = "C-R00012";
        private const EveSystemType SOLAR_SYSTEM_WH_CLASS = EveSystemType.C3;
        private const WHEffect SOLAR_SYSTEM_WH_EFFECT = WHEffect.Pulsar;
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";

        private const string USERNAME1 = "FOOBAR1";
        private const string USERNAME2 = "FOOBAR2";

        public GraphModelsTest()
        {
        }

        [Fact]
        public async Task Eve_System_Node_Model()
        {
            var node = new EveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, SOLAR_SYSTEM_EXTENSION_NAME, SOLAR_SYSTEM_JITA_SECURITY), new WHNote(SOLAR_SYSTEM_JITA_ID,WHSystemStatusEnum.Friendly,SOLAR_SYSTEM_JITA_NAME), REGION_JITA_NAME, CONSTELLATION_JITA_NAME);
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


            node.IncrementNameExtension();
            Assert.Equal("C",node.NameExtension);
            for(int i=0; i<26;i++)
                node.IncrementNameExtension();
            Assert.Equal("Z",node.NameExtension);

            node.DecrementNameExtension();
            Assert.Equal("Y",node.NameExtension);
            for(int i=0; i<26;i++)
                node.DecrementNameExtension();
            Assert.Null(node.NameExtension);

            node.SystemStatus=WHSystemStatusEnum.Hostile;
            Assert.Equal(WHSystemStatusEnum.Hostile,node.SystemStatus);

            node.Locked=true;
            Assert.True(node.Locked);

            Assert.False(node.IsRouteWaypoint);
            node.IsRouteWaypoint=true;
            Assert.True(node.IsRouteWaypoint);

            var node_ls = new EveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_AMAMAKE_ID, SOLAR_SYSTEM_AMAMAKE_NAME, SOLAR_SYSTEM_AMAMAKE_SECURITY), null, REGION_AMAMAKE_NAME, CONSTELLATION_AMAMAKE_NAME); 
            Assert.Equal(EveSystemType.LS, node_ls.SystemType);

            var node_ns = new EveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_6_CZ49_ID, SOLAR_SYSTEM_6_CZ49_NAME, SOLAR_SYSTEM_6_CZ49_SECURITY), null, REGION_6_CZ49_NAME, CONSTELLATION_6_CZ49_NAME); 
            Assert.Equal(EveSystemType.NS, node_ns.SystemType);
        }

        [Fact]
        public void Eve_System_Link_Model()
        {
            var node = new EveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID,SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, SOLAR_SYSTEM_JITA_SECURITY), null, REGION_JITA_NAME, CONSTELLATION_JITA_NAME);
                Assert.Equal(WHSystemStatusEnum.Unknown,node.SystemStatus);
            var node2 = new EveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, -1.0F), null, REGION_WH_NAME, CONSTELLATION_WH_NAME,SOLAR_SYSTEM_WH_CLASS, SOLAR_SYSTEM_WH_EFFECT,null, new List<WHStatic>() { new WHStatic(SOLAR_SYSTEM_WH_STATICS,EveSystemType.C3) }) ;
   

            var link = new EveSystemLinkModel(new WHSystemLink(1, 2), node, node2);
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
            Assert.False(link.IsRouteWaypoint);
            link.IsRouteWaypoint=true;
            Assert.True(link.IsRouteWaypoint);

            link.IsEoL=true;
            Assert.True(link.IsEoL);
        }
    }
}

