using System.Reflection.Emit;
using System.Xml.Linq;
using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.SDE;
using WHMapper.Services.WHColor;
using static MudBlazor.CategoryTypes;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemNode : ComponentBase
    {
        private const string MENU_LOCK_SYSTEM_VALUE = "Lock";
        private const string MENU_UNLOCK_SYSTEM_VALUE = "Unlock";


        private EveSystemNodeModel _node = null!;

        private string _secColor = IWHColorHelper.DEFAULT_COLOR;
        private string _systemColor = IWHColorHelper.DEFAULT_COLOR;
        private string _whEffectColor = IWHColorHelper.DEFAULT_COLOR;
        private string _menu_lock_value = MENU_LOCK_SYSTEM_VALUE;
        private string _menu_lock_icon_value = Icons.Material.Sharp.Lock;


        [Inject]
        IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        IWHSystemRepository DbWHSystems { get; set; } = null!;

        [Inject]
        IWHNoteRepository DbNotes { get; set; } = null!;

        [Inject]
        public ILogger<EveSystemNode> Logger { get; set; } = null!;


        [Inject]
        public IWHColorHelper WHColorHelper { get; set; } = null!;

        
        [ParameterAttribute]
        public EveSystemNodeModel Node
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;
                if(_node!=null)
                {
                    _secColor = WHColorHelper.GetSecurityStatusColor(_node.SecurityStatus);
                    _systemColor = WHColorHelper.GetSystemTypeColor(_node.SystemType);
                    _whEffectColor = WHColorHelper.GetEffectColor(_node.Effect);

                    Locked = _node.Locked;

                }
            }
        }

        private bool Locked
        {
            get
            {
                if (_node != null)
                    return _node.Locked;
                else
                    return false;
            }
            set
            {
                if(_node!=null)
                {
                    _node.Locked = value;

                    if (_node.Locked)
                    {
                        _menu_lock_value = MENU_UNLOCK_SYSTEM_VALUE;
                        _menu_lock_icon_value = Icons.Material.Sharp.LockOpen;
                    }
                    else
                    {
                        _menu_lock_value = MENU_LOCK_SYSTEM_VALUE;
                        _menu_lock_icon_value = Icons.Material.Sharp.Lock;
                    }
                    StateHasChanged();
                }
            }
        }

        private String systemStyle
        {
            get
            {
                String systemStyle = "";

                if (Node.Selected)
                {
                    systemStyle += " box-shadow: 0px 0px 12px #fff;";
                }
                var sysStatus = Node.SystemStatus;

                if (sysStatus == Models.Db.Enums.WHSystemStatusEnum.Unknown)
                {
                    systemStyle += " border-color:grey;";
                }

                if (sysStatus == Models.Db.Enums.WHSystemStatusEnum.Friendly)
                {
                    systemStyle += " border-color:royalblue;";
                }

                if (sysStatus == Models.Db.Enums.WHSystemStatusEnum.Occupied)
                {
                    systemStyle += " border-color:orange;";
                }

                if (sysStatus == Models.Db.Enums.WHSystemStatusEnum.Hostile)
                {
                    systemStyle += " border-color:red;";
                }

                return systemStyle;
            }
        }

        private async Task<bool> SetSelectedSystemDestinationWaypoint()
        {
            try
            {
                if (_node.Selected)
                {
                    var res = await EveServices.UserInterfaceServices.SetWaypoint(_node.SolarSystemId, false, true);
                    return true;

                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set destination waypoint error");
                return false;
            }
        }

        private async Task<bool> SetSelectedSystemStatus(WHSystemStatusEnum systemStatus)
        {
            try
            {
                if (_node.Selected)
                {
                    var note = await DbNotes.GetBySolarSystemId(_node.SolarSystemId);

                    bool success = false;

                    if(note == null)
                    {
                        var newNote = new WHNote(_node.SolarSystemId, systemStatus);
                        await DbNotes.Create(newNote);
                        success = true;
                    }
                    else
                    {
                        note.SystemStatus = systemStatus;
                        await DbNotes.Update(note.Id, note);
                        success = true;
                    }
                    
                    if(success)
                    {
                        _node.SystemStatus = systemStatus;
                        _node.Refresh();
                        return true;
                    }
                    else
                    {
                        Logger.LogError("Could not update system status");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set system status error");
                return false;
            }
        }

        private async Task<bool> ToggleSystemLock()
        {
            try
            {
                var whSystem = await DbWHSystems.GetById(_node.IdWH);
                if (whSystem != null && whSystem.Id==_node.IdWH)
                {
                    whSystem.Locked = !whSystem.Locked;
                    whSystem = await DbWHSystems.Update(whSystem.Id, whSystem);
                    if (whSystem == null)
                    {
                        Logger.LogError("Update lock system status error");
                        return false;
                    }

                    Locked = whSystem.Locked;
                    _node.Refresh();
                    return true;
                }
                else
                {
                    return false;
                }
          

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Toggle system lock error");
                return false;
            }
         
        }

        public static implicit operator EveSystemNode?(NodeModel? v)
        {
            throw new NotImplementedException();
        }
    }
}

