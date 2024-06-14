using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHNotes;


namespace WHMapper.Pages.Mapper.Notes
{
    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase,IDisposable
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
        private WHSystemStatus _systemStatus = WHSystemStatus.Unknown;

        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

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

        protected Task OnNoteChanged()
        {
            if (_timer == null)
            {
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
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
                    _cts = new CancellationTokenSource();
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        if(string.IsNullOrEmpty(_solarSystemComment) && _systemStatus.Equals(WHSystemStatus.Unknown))
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
                                    Logger.LogError(autoDelNote,MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR);
                                    Snackbar.Add(MSG_SOLAR_SYSTEM_COMMENT_AUTODEL_ERROR, Severity.Error);
                                }
                            }

                            await _cts.CancelAsync();
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
                                        if(_note!=null)
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

                                await _cts.CancelAsync();
                            }
                            else
                            {
                                _previousValue = _solarSystemComment;
                            }
                        }
                    }
                }
                catch(OperationCanceledException oce)
                {
                    Logger.LogInformation(oce,"Operation canceled");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,MSG_AUTOSAVE_OR_UPDATE_ERROR);
                }
                finally
                {
                    Dispose(true);
                    _previousValue = string.Empty;
                }
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
    