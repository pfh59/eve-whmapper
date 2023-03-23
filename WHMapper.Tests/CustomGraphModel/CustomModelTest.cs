using System;
using System.Xml.Linq;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Tests.CustomGraphModel
{
    [TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.CustomGraphModel")]
    public class CustomModelTest
    {
        private const string SOLAR_SYSTEM_JITA_NAME = "Jita";

        private const string SOLAR_SYSTEM_WH_NAME = "J165153";
        private const string SOLAR_SYSTEM_WH_CLASS = "C3";
        private const string SOLAR_SYSTEM_WH_EFFECT = "Pulsar";
        private const string SOLAR_SYSTEM_WH_STATICS = "D845";

        private const string USERNAME1 = "FOOBAR1";
        private const string USERNAME2 = "FOOBAR2";


        public CustomModelTest()
        {


        }


        [Fact]
        public async Task Eve_System_Node_Model()
        {
            var node = new EveSystemNodeModel(new Models.Db.WHSystem(SOLAR_SYSTEM_JITA_NAME, 1.0F));
            Assert.NotNull(node);
            Assert.Equal(0, node.IdWH);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, node.Title);
            Assert.Equal("H", node.Class);
            Assert.Empty(node.ConnectedUsers);
            Assert.Null(node.NameExtension);

            node.AddConnectedUser(USERNAME1);
            node.AddConnectedUser(USERNAME2);
            Assert.Contains(USERNAME1, node.ConnectedUsers);
            Assert.Contains(USERNAME2, node.ConnectedUsers);

            node.RemoveConnectedUser(USERNAME2);
            Assert.Contains(USERNAME1, node.ConnectedUsers);
            Assert.DoesNotContain(USERNAME2, node.ConnectedUsers);
        }

        [Fact]
        public async Task Eve_System_Link_Model()
        {
            var node = new EveSystemNodeModel(new Models.Db.WHSystem(SOLAR_SYSTEM_JITA_NAME, 1.0F));
            var node2 = new EveSystemNodeModel(new Models.Db.WHSystem(SOLAR_SYSTEM_WH_NAME, -1.0F), SOLAR_SYSTEM_WH_CLASS, SOLAR_SYSTEM_WH_EFFECT, null, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(SOLAR_SYSTEM_WH_STATICS, "HS") });


            var link = new EveSystemLinkModel(new Models.Db.WHSystemLink(1, 2), node, node2);
            Assert.NotNull(link);
            Assert.Equal(SOLAR_SYSTEM_JITA_NAME, ((EveSystemNodeModel)link.Source.Model).Name);
            Assert.Equal(SOLAR_SYSTEM_WH_NAME, ((EveSystemNodeModel)link.Target.Model).Name);
            Assert.False(link.IsEoL);
            Assert.Equal(SystemLinkMassStatus.Normal, link.MassStatus);
            Assert.Equal(SystemLinkSize.Large, link.Size);

        }
    }
}

