using System.ComponentModel;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.WHColor;


namespace WHMapper.Pages.Mapper.Notes
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase
    {
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_SUCCESS = "Solar system comment autosave successfull";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_ERROR = "Solar system comment autosave error";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_SUCCESS = "Solar system comment autodelete successfull";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR = "Solar system comment autodelete error";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_SUCCESS = "Solar system comment autoupdate successfull";
        private const string MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR = "Solar system comment autoupdate error";
        private const string MSG_AUTOSAVE_OR_UPDATE_ERROR = "Solar system autosave or update error";

        private WHNote? _note = null!;
        private string _solarSystemComment = string.Empty;
        private WHSystemStatusEnum _systemStatus = WHSystemStatusEnum.Unknown;

        private PeriodicTimer _timer = null!;
        private CancellationTokenSource _cts = null!;

        private string _previousValue = string.Empty;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IWHNoteRepository DbWHNotes { get; set; } = null!;

        [Parameter]
        public EveSystemNodeModel CurrentSystemNode { get; set; } = null!;

        protected async override Task OnParametersSetAsync()
        {
            if (CurrentSystemNode != null)
            {
                _solarSystemComment = string.Empty;
                _previousValue=string.Empty;
                //load system saved notes
                if (DbWHNotes != null)
                {
                    _note = await DbWHNotes.GetBySolarSystemId(CurrentSystemNode.SolarSystemId);
                    if (_note == null)
                        _solarSystemComment = string.Empty;
                    else
                    {
                        _solarSystemComment = _note.Comment;
                        _systemStatus = _note.SystemStatus;
                    }
                }
            }
        }

        private Task OnNoteChanged()
        {
            if (_timer == null)
            {
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
                _cts = new CancellationTokenSource();
                Task.Run(() => HandleTimerAsync());
            }
            return Task.CompletedTask;
        }

        private async Task HandleTimerAsync()
        {

            if (_timer != null)
            {
                _previousValue = _solarSystemComment;

                try
                {
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        if(string.IsNullOrEmpty(_solarSystemComment) && _systemStatus.Equals(WHSystemStatusEnum.Unknown))
                        {
                            if(_note!=null)
                            {
                                try
                                {
                                    if(await DbWHNotes.DeleteById(_note.Id))
                                    {
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_SUCCESS, Severity.Success);
                                        _note=null;
                                    }
                                    else
                                    {
                                        Logger.LogError(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR);
                                        Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR, Severity.Error);
                                    }
                                }
                                catch(Exception autoDelNote)
                                {
                                    Logger.LogError(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR, autoDelNote);
                                    Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR, Severity.Error);
                                }
                            }

                            _cts.Cancel();
                        }
                        else
                        {
                            if(_previousValue==_solarSystemComment)
                            {
                                
                                var note = await DbWHNotes.GetBySolarSystemId(CurrentSystemNode.SolarSystemId);

                                if (note == null)
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
                                        Logger.LogError(autoSaveEx,MSG_SOLAR_SYSTEM_COMMENT_AUTOSAVE_ERROR);
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
                                        Logger.LogError(exAutoSave,MSG_SOLAR_SYSTEM_COMMENT_AUTOUPDATE_ERROR);
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
    