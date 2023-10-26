using System.ComponentModel;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.WHColor;


namespace WHMapper.Pages.Mapper.SystemInfos
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {        
        private const string NO_EFFECT = "No Effect";

        private string _secColor = string.Empty;
        private string _systemColor= string.Empty;
        private string _whEffectColor = string.Empty;

        private string _systemType = string.Empty;
        private string _effect = NO_EFFECT;
        [Inject]
        private IWHNoteRepository DbWHNotes { get; set; } = null!;

        [Inject]
        private IWHColorHelper WHColorHelper { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;


        [Parameter]
        public EveSystemNodeModel CurrentSystemNode { get; set; } = null!;


        protected async override Task OnParametersSetAsync()
        {

            if (CurrentSystemNode != null)
            {
                _secColor = WHColorHelper.GetSecurityStatusColor(CurrentSystemNode.SecurityStatus);
                _systemColor = WHColorHelper.GetSystemTypeColor(CurrentSystemNode.SystemType);
                _whEffectColor = WHColorHelper.GetEffectColor(CurrentSystemNode.Effect);


                switch (CurrentSystemNode.SystemType)
                {
                    case EveSystemType.Pochven:
                        _systemType = "T";
                        break;
                    case EveSystemType.None:
                        _systemType = string.Empty; ;
                        break;
                    default:
                        _systemType = CurrentSystemNode.SystemType.ToString();
                        break;
                }

                if (CurrentSystemNode.Effect == WHEffect.None)
                    _effect = NO_EFFECT;
                else
                    _effect = GetWHEffectValueAsString(CurrentSystemNode.Effect);
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

