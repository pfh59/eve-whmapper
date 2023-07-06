using System.Drawing;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;
using Color = MudBlazor.Color;

namespace WHMapper.Pages.Mapper.SystemInfos
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {
        private bool _showWHInfos=false;

        private EveSystemNodeModel _currentSystemNode = null!;

        private string _secColor = string.Empty;
        private string _systemColor= string.Empty;
        private string _whEffectColor = string.Empty;


        [Inject]
        public IWHColorHelper WHColorHelper { get; set; } = null!;

        [Parameter]
        public EveSystemNodeModel CurrentSystemNode
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
                    _showWHInfos = ((bool)(_currentSystemNode.Class.Contains('C')) ? true : false);
                    _secColor = WHColorHelper.GetSecurityStatusColor(_currentSystemNode.SecurityStatus);
                    _systemColor = WHColorHelper.GetSystemTypeColor(_currentSystemNode.Class);
                    _whEffectColor = WHColorHelper.GetEffectColor(_currentSystemNode.Effect);

                }
            }

        }
    }
}

