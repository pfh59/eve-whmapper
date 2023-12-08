using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.WHColor;
using WHMapper.Services.WHSignature;
using static MudBlazor.CategoryTypes;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;

namespace WHMapper.Pages.Mapper.Signatures
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase,IAsyncDisposable
    {
        private const int ESI_WH_GROUPE_ID = 988;

        [Inject]
        private IWHSignatureHelper SignatureHelper { get; set; } = null!;

        [Inject]
        private IEveUserInfosServices UserInfos { get; set; } = null!;

        [Inject]
        private IWHSignatureRepository DbWHSignatures { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        //[Inject]
        //private IAnoikServices AnoikServices { get; set; } = null!;

        [Inject]
        private IEveMapperHelper EveMapperHelperServices { get; set; } = null!;
        

        [Inject]
        private IWHColorHelper WHColorHelper { get; set; } = null!;

        private IEnumerable<WHSignature> Signatures { get; set; } = null!;


        [Parameter]
        public int? CurrentMapId {  get; set; } = null!;
        [Parameter]
        public int? CurrentSystemNodeId {  get; set; } = null!;
        [Parameter]
        public HubConnection NotificationHub { private get; set; } = null!;


        private WHSignature _selectedSignature = null!;
        private WHSignature _signatureBeforeEdit = null!;

        private bool _isEditingSignature = false;

        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;
        private DateTime _currentDateTime;


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await Restore();
            HandleTimerAsync();
            await base.OnParametersSetAsync();
        }



        private async Task HandleTimerAsync()
        {

            if (_timer == null)
            {

                _cts = new CancellationTokenSource();
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
                _currentDateTime = DateTime.UtcNow;
                try
                {
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        _currentDateTime = DateTime.UtcNow;
                        StateHasChanged();
                    }
                }
                catch (Exception ex)
                {

                    //Handle the exception but don't propagate it
                }
            }
        }

        private string DateDiff(DateTime startTime, DateTime endTime)
        {
            TimeSpan span = startTime.Subtract(endTime);
            if (span.Days > 0)
                return String.Format("{0}d {1}h {2}m {3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
            else if (span.Days == 0 && span.Hours > 0)
                return String.Format("{0}h {1}m {2}s", span.Hours, span.Minutes, span.Seconds);
            else if (span.Days == 0 && span.Hours == 0 && span.Minutes > 0)
                return String.Format("{0}m {1}s", span.Minutes, span.Seconds);
            else
                return String.Format("{0}s", span.Seconds);
        }

        private string GetDisplayText(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            System.Type type = value.GetType();
            if (System.Attribute.IsDefined(type, typeof(FlagsAttribute)))
            {
                var sb = new System.Text.StringBuilder();

                foreach (Enum field in Enum.GetValues(type))
                {
                    if (Convert.ToInt64(field) == 0 && Convert.ToInt32(value) > 0)
                        continue;

                    if (value.HasFlag(field))
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        var f = type.GetField(field.ToString());
                        var da = (DisplayAttribute)System.Attribute.GetCustomAttribute(f, typeof(DisplayAttribute));
                        sb.Append(da?.ShortName ?? da?.Name ?? field.ToString());
                    }
                }

                return sb.ToString();
            }
            else
            {
                var f = type.GetField(value.ToString());
                if (f != null)
                {
                    var da = (DisplayAttribute)System.Attribute.GetCustomAttribute(f, typeof(DisplayAttribute));
                    if (da != null)
                        return da.ShortName ?? da.Name;
                }
            }

            return value.ToString();
        }

        private async Task  OpenImportDialog()
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                DisableBackdropClick = true,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId);

            var dialog = DialogService.Show<Import>("Import Scan Dialog", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && CurrentMapId!=null && CurrentSystemNodeId!=null)
            {
                await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
            
        }

        public async Task Restore()
        {
            if (DbWHSignatures!=null && CurrentSystemNodeId != null && CurrentSystemNodeId > 0)
            {
                var sigs = await DbWHSignatures.GetByWHId(CurrentSystemNodeId.Value);
                if(sigs!=null)
                    Signatures = sigs.ToList();
                else
                    Signatures = new List<WHSignature>();
            }
            else
            {
                Signatures = new List<WHSignature>();
            }
        }

        private async Task DeleteSignature(int id)
        {
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId.Value);
            parameters.Add("SignatureId", id);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService.Show<Delete>("Delete", parameters, options);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && CurrentMapId!=null && CurrentSystemNodeId!=null)
            {
                await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
        }


        private async Task DeleteAllSignature()
        {
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId.Value);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService.Show<Delete>("Delete", parameters, options);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && CurrentMapId != null && CurrentSystemNodeId != null)
            {
                await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
        }
    
        private void BackupSingature(object element)
        {
            _isEditingSignature = true;

            _signatureBeforeEdit = new WHSignature(
                ((WHSignature)element).WHId,
                ((WHSignature)element).Name,
                ((WHSignature)element).Group,
                ((WHSignature)element).Type
                );

            StateHasChanged();
        }

        private void ResetSingatureToOriginalValues(object element)
        {
           

            ((WHSignature)element).Name = _signatureBeforeEdit.Name;
            ((WHSignature)element).Group = _signatureBeforeEdit.Group;
            ((WHSignature)element).Type = _signatureBeforeEdit.Type;

            _isEditingSignature = false;
            StateHasChanged();
        }

        private async void SignatiureHasBeenCommitted(object element)
        {
            ((WHSignature)element).Updated = DateTime.UtcNow;
            ((WHSignature)element).UpdatedBy = await UserInfos.GetUserName();

            var res = await DbWHSignatures.Update(((WHSignature)element).Id, ((WHSignature)element));

            if(res!=null && res.Id== ((WHSignature)element).Id)
                Snackbar.Add("Signature successfully updated", Severity.Success);
            else
                Snackbar.Add("No signature updated", Severity.Error);

            _isEditingSignature = false;
            StateHasChanged();
        }

        private void Cancel()
        {
            _cts?.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            Cancel();
            _timer?.Dispose();

            GC.SuppressFinalize(this);
        }

        private async Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId)
        {
            if (NotificationHub is not null)
            {
                await NotificationHub.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
            }
        }
    }
}

