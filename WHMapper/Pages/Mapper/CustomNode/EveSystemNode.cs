using System.Reflection.Emit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.EveAPI;
using WHMapper.Services.SDE;
using WHMapper.Services.WHColor;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemNode : ComponentBase
    {
        private string _secColor;
        private string _systemColor;
        private string _whEffectColor;

        [Inject]
        IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        public ILogger<EveSystemNode> Logger { get; set; } = null!;


        [Inject]
        public IWHColorHelper? WHColorHelper { get; set; }

        private EveSystemNodeModel _node;
        [ParameterAttribute]
        public EveSystemNodeModel Node
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;
                if(_node!=null)
                {
                    _secColor = WHColorHelper?.GetSecurityStatusColor(_node.SecurityStatus);
                    _systemColor = WHColorHelper?.GetSystemTypeColor(_node.Class);
                    _whEffectColor = WHColorHelper?.GetEffectColor(_node.Effect);
                }
            }
        }

        public async Task<bool> SetSelectedSystemDestinationWaypoint()
        {
            try
            {
                if (_node.Selected)
                {
                    var res = await EveServices.UserInterfaceServices.SetWaypoint(_node.SolarSystemId, false, true);
                    return true;

                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set destination waypoint error");
                return false;
            }
        }
    }
}

