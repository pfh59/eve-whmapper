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
    public partial class Overview : ComponentBase,IDisposable
    {
        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IEveUserInfosServices UserInfos { get; set; } = null!;

        [Inject]
        private IWHSignatureRepository DbWHSignatures { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        protected IEveMapperHelper EveMapperHelperServices { get; set; } = null!;
        
        [Inject]
        protected IWHColorHelper WHColorHelper { get; set; } = null!;

        private IEnumerable<WHSignature> Signatures { get; set; } = null!;


        [Parameter]
        public int? CurrentMapId {  get; set; } = null!;
        [Parameter]
        public int? CurrentSystemNodeId {  get; set; } = null!;
        [Parameter]
        public HubConnection NotificationHub { private get; set; } = null!;


        protected WHSignature? _selectedSignature;
        private WHSignature _signatureBeforeEdit = null!;

        protected bool _isEditingSignature = false;

        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;
        protected DateTime _currentDateTime;

        private MudTable<WHSignature> _signatureTable { get; set; } =null!;

        private string? _currentUser;


        protected override Task OnParametersSetAsync()
        {
            Task.WhenAll(Task.Run(() => Restore()), Task.Run(() => HandleTimerAsync()));
            return base.OnParametersSetAsync();
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
                        await InvokeAsync(() => {
                            StateHasChanged();
                        });
                    }
                }
                catch (OperationCanceledException oce)
                {
                    Logger.LogInformation(oce,"Operation canceled");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in timer");
                }
                finally
                {
                    Dispose(true);
                }
            }
        }

        protected string DateDiff(DateTime startTime, DateTime endTime)
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

        protected string GetDisplayText(Enum value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            var type = value.GetType();
            return System.Attribute.IsDefined(type, typeof(FlagsAttribute)) ? GetFlagsDisplayText(value, type) : GetFieldDisplayName(value);
        }

        private string GetFlagsDisplayText(Enum value, System.Type type)
        {
            var displayTexts = Enum.GetValues(type).Cast<Enum>()
                .Where(field => value.HasFlag(field) && Convert.ToInt64(field) != 0)
                .Select(GetFieldDisplayName);

            return string.Join(", ", displayTexts);
        }

        private string GetFieldDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field is null)
                return value.ToString();
            
            var attribute = System.Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            if(attribute is null)
                return value.ToString();

            DisplayAttribute displayAttribute = (DisplayAttribute)attribute!;    

            return displayAttribute.ShortName ?? displayAttribute.Name ?? value.ToString();
        }

        protected async Task  OpenImportDialog()
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                BackdropClick=false,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId);

            var dialog = await DialogService.ShowAsync<Import>("Import Scan Dialog", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && CurrentMapId!=null && CurrentSystemNodeId!=null)
            {
                await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
            
        }

        public async Task Restore()
        {
            if(String.IsNullOrEmpty(_currentUser))
                _currentUser = await UserInfos.GetUserName();

            if(_signatureTable is not null)
                _signatureTable.SetEditingItem(null);


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
        protected async Task DeleteSignature(int id)
        {
            if(CurrentSystemNodeId!=null)
            {
                var parameters = new DialogParameters();
                parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId.Value);
                parameters.Add("SignatureId", id);

                var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

                var dialog = await DialogService.ShowAsync<Delete>("Delete", parameters, options);
                DialogResult result = await dialog.Result;

                if (!result.Canceled && CurrentMapId!=null)
                {
                    await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                    await Restore();
                }
            }
            else
            {
                Logger.LogError("No system selected");
                Snackbar.Add("No system selected", Severity.Error);
            }
        }

        protected async Task DeleteAllSignature()
        {
            var parameters = new DialogParameters();
            if(CurrentSystemNodeId!=null && CurrentMapId!=null)
                parameters.Add("CurrentSystemNodeId", CurrentSystemNodeId.Value);
            else
            {
                Logger.LogError("No system selected");
                Snackbar.Add("No system selected", Severity.Error);
                return;
            }
    
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = await DialogService.ShowAsync<Delete>("Delete", parameters, options);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && CurrentMapId != null && CurrentSystemNodeId != null)
            {
                await NotifyWormholeSignaturesChanged(CurrentMapId.Value, CurrentSystemNodeId.Value);
                await Restore();
            }
        }
    
        protected void BackupSingature(object element)
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

        protected void ResetSingatureToOriginalValues(object element)
        {
            ((WHSignature)element).Name = _signatureBeforeEdit.Name;
            ((WHSignature)element).Group = _signatureBeforeEdit.Group;
            ((WHSignature)element).Type = _signatureBeforeEdit.Type;

            _isEditingSignature = false;
            StateHasChanged();
        }

        protected void SignatiureHasBeenCommitted(object element)
        {
            ((WHSignature)element).Updated = DateTime.UtcNow;
            ((WHSignature)element).UpdatedBy = _currentUser;

            Task.Run(() => UpdateSignature(element));

            _isEditingSignature = false;
            StateHasChanged();
        }

        private async Task UpdateSignature(object element)
        {
            var res = await DbWHSignatures.Update(((WHSignature)element).Id, ((WHSignature)element));
            if(res!=null && res.Id== ((WHSignature)element).Id)
                Snackbar.Add("Signature successfully updated", Severity.Success);
            else
                Snackbar.Add("No signature updated", Severity.Error);
        }

        private async Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId)
        {
            if (NotificationHub is not null)
            {
                await NotificationHub.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._timer?.Dispose();
            this._cts?.Dispose();
        }
    }
}

