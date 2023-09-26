using System.Reflection.Emit;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.SDE;
using WHMapper.Services.WHColor;
using static MudBlazor.CategoryTypes;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemNode : ComponentBase
    {
        private EveSystemNodeModel _node = null!;

        private string _secColor = IWHColorHelper.DEFAULT_COLOR;
        private string _systemColor = IWHColorHelper.DEFAULT_COLOR;
        private string _whEffectColor = IWHColorHelper.DEFAULT_COLOR;


        [Inject]
        public ILogger<EveSystemNode> Logger { get; set; } = null!;


        [Inject]
        public IWHColorHelper WHColorHelper { get; set; } = null!;

        
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
                    _secColor = WHColorHelper.GetSecurityStatusColor(_node.SecurityStatus);
                    _systemColor = WHColorHelper.GetSystemTypeColor(_node.SystemType);
                    _whEffectColor = WHColorHelper.GetEffectColor(_node.Effect);
                }
            }
        }
    }
}

