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
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_SUCCESS = "Solar system comment autosave successfull";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_ERROR = "Solar system comment autosave error";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_SUCCESS = "Solar system comment autoupdate successfull";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR = "Solar system comment autoupdate error";
        private const string MSG_AUTOSAVE_OR_UPDATE_ERROR = "Solar system autosave or update error";

        private const string NO_EFFECT = "No Effect";

        private string _secColor = string.Empty;
        private string _systemColor= string.Empty;
        private string _whEffectColor = string.Empty;

        private string _systemType = string.Empty;
        private string _effect = NO_EFFECT;

        private string _solarSystemComment = string.Empty;
        private WHNote? _note = null!;


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
                _solarSystemComment = string.Empty;
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


                //load system saved notes
                if (DbWHNotes != null)
                {
                    _note = await DbWHNotes.GetBySolarSystemId(CurrentSystemNode.SolarSystemId);
                    if (_note == null)
                        _solarSystemComment = string.Empty;
                    else
                        _solarSystemComment = _note.Comment;
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

        private async Task OnNoteChanged()
        {
            if (_timer == null && !string.IsNullOrEmpty(_solarSystemComment))
            {
                Task.Run(() => HandleTimerAsync());
            }
        }

        private PeriodicTimer _timer = null!;
        private CancellationTokenSource _cts = null!;

        private string _previousValue;

        private async Task HandleTimerAsync()
        {

            if (_timer == null && !string.IsNullOrEmpty(_solarSystemComment))
            {
                _previousValue = _solarSystemComment;
                _cts = new CancellationTokenSource();
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
                try
                {
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        if(_previousValue== _solarSystemComment)
                        {
                            if (_note == null)
                            {
                                try
                                {
                                    _note = await DbWHNotes.Create(new WHNote(CurrentSystemNode.SolarSystemId, _solarSystemComment));
                                    if (_note != null)
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_SUCCESS, Severity.Success);
                                    else
                                    {
                                        Logger.LogError(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR);
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_ERROR, Severity.Error);
                                    }
                                }
                                catch(Exception autoSaveEx)
                                {

                                    Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_ERROR, Severity.Error);
                                    
                                }
                            }
                            else
                            {
                                try
                                {
                                    _note.Comment = _solarSystemComment;
                                    _note = await DbWHNotes.Update(_note.Id, _note);
                                    if (_note != null)
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_SUCCESS, Severity.Success);
                                    else
                                    {
                                        Logger.LogError(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR);
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR, Severity.Error);
                                    }
                                }
                                catch (Exception exAutoSave)
                                {
                                    Logger.LogError(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR, exAutoSave);
                                    Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR, Severity.Error);
                                }
                            }

                            _cts.Cancel();
                        }
                        else
                        {
                            _previousValue = _solarSystemComment;
                        }
                    }
                }
                catch(OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Logger.LogError(MSG_AUTOSAVE_OR_UPDATE_ERROR, ex);
                }
                finally
                {
                    _timer = null!;
                    _cts = null!;
                    _previousValue = string.Empty;
                }
            }
        }

    }
}

