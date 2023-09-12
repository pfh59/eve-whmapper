using System.ComponentModel;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.WHColor;
using Color = MudBlazor.Color;

namespace WHMapper.Pages.Mapper.SystemInfos
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {
        private const string NO_EFFECT = "No Effect";


        private EveSystemNodeModel _currentSystemNode = null!;

        private string _secColor = string.Empty;
        private string _systemColor= string.Empty;
        private string _whEffectColor = string.Empty;

        private string _systemType = string.Empty;
        private string _effect = NO_EFFECT;


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
                if (_currentSystemNode != null)
                {
                    _secColor = WHColorHelper.GetSecurityStatusColor(_currentSystemNode.SecurityStatus);
                    _systemColor = WHColorHelper.GetSystemTypeColor(_currentSystemNode.SystemType);
                    _whEffectColor = WHColorHelper.GetEffectColor(_currentSystemNode.Effect);



                    switch(_currentSystemNode.SystemType)
                    {
                        case EveSystemType.Pochven:
                            _systemType = "T";
                            break;
                        case EveSystemType.None:
                            _systemType = string.Empty; ;
                            break;
                        default:
                            _systemType = _currentSystemNode.SystemType.ToString();
                            break;


                    }

                    if (_currentSystemNode.Effect == WHEffect.None)
                        _effect = NO_EFFECT;
                    else
                        _effect = GetWHEffectValueAsString(_currentSystemNode.Effect);
                }
            }
        }

        private string GetWHEffectValueAsString(WHEffect effect)
        {
            var field = effect.GetType().GetField(effect.ToString());
            var customAttributes = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (customAttributes != null && customAttributes.Length > 0)
            {
                return (customAttributes[0] as DescriptionAttribute).Description;
            }
            else
            {
                return effect.ToString();
            }
        }


    }
}

