using System.Drawing;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;
using Color = MudBlazor.Color;

namespace WHMapper.Pages.Mapper.SystemInfos
{
    public partial class Overview : ComponentBase
    {
        private bool _showWHInfos=false;

        private EveSystemNodeModel? _currentSystemNode;

        private string _secColor;
        private string _systemColor;
        private string _whEffectColor;


        [Inject]
        public IWHColorHelper? WHColorHelper { get; set; }

        [Parameter]
        public EveSystemNodeModel? CurrentSystemNode
        {
            get
            {
                return _currentSystemNode;
            }
            set
            {
                _currentSystemNode = value;
                if (_currentSystemNode == null)
                    _showWHInfos = false;
                else
                {
                    _showWHInfos = ((bool)(_currentSystemNode?.Class?.Contains('C')) ? true : false);
                    _secColor = WHColorHelper?.GetSecurityStatusColor(_currentSystemNode.SecurityStatus);
                    _systemColor = WHColorHelper?.GetSystemTypeColor(_currentSystemNode.Class);
                    _whEffectColor = WHColorHelper?.GetEffectColor(_currentSystemNode.Effect);

                }
            }

        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

    }
}

