using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemNode : ComponentBase
    {
        private string _secColor;
        private string _systemColor;
        private string _whEffectColor;




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
    }
}

