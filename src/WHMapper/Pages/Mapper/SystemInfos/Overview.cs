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
        private const string DOTLAN_URL="https://evemaps.dotlan.net/system/{0}";
        private const string DOTLAN_LOGO_PATH="/Images/logo_dotlan.png";
        private const string ZKILLBOARD_URL="https://zkillboard.com/system/{0}";
        private const string ZKILLBOARD_LOGO_PATH="/Images/logo_zkillboard.png";
        private const string ANOIK_URL="http://anoik.is/systems/{0}";
        private const string ANOIK_LOGO_PATH="/Images/logo_anoik.png";

        private string _secColor = string.Empty;
        private string _systemColor= string.Empty;
        private string _whEffectColor = string.Empty;

        private string _linkToDotlan=  string.Empty;
        private string _linkToZKillboard=  string.Empty;
        private string _linkToAnoik = string.Empty;
        private string _systemType = string.Empty;
        private float _secStatus = 0;

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


        protected override Task OnParametersSetAsync()
        {

            if (CurrentSystemNode != null)
            {
                _secColor = WHColorHelper.GetSecurityStatusColor(CurrentSystemNode.SecurityStatus);
                _systemColor = WHColorHelper.GetSystemTypeColor(CurrentSystemNode.SystemType);
                _whEffectColor = WHColorHelper.GetEffectColor(CurrentSystemNode.Effect);


                _linkToDotlan = string.Format(DOTLAN_URL,CurrentSystemNode.Name);
                _linkToZKillboard = string.Format(ZKILLBOARD_URL,CurrentSystemNode.SolarSystemId);
                _linkToAnoik = string.Format(ANOIK_URL,CurrentSystemNode.Name); 


                _secStatus=(float)Math.Round(CurrentSystemNode.SecurityStatus , 1);
                if(_secStatus==-0.0f)
                    _secStatus=0.00f;

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

            }
            return base.OnParametersSetAsync();
        }
    }
}

