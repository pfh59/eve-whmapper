using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.WHColor;


namespace WHMapper.Components.Pages.Mapper.CustomNode;

public partial class EveSystemNode
{
    private const int MAX_BADGE_LABEL_LENGTH = 3;
    private const string LONG_BADGE_LABEL_FONT_SIZE = "0.5rem";

    private string _systemColor = IWHColorHelper.DEFAULT_COLOR;
    private string _whEffectColor = IWHColorHelper.DEFAULT_COLOR;

    private bool _isEditingName = false;
    private string? _editedName;

    [Inject]
    ILogger<EveSystemNode> Logger { get; set; } = null!;

    [Inject]
    IWHColorHelper WHColorHelper { get; set; } = null!;

    [ParameterAttribute]
    public EveSystemNodeModel Node { get; set; } = null!;

    private bool Locked
    {
        get
        {
            if (Node != null)
                return Node.Locked;
            else
                return false;
        }
        set
        {
            if (Node != null)
            {
                Node.Locked = value;
                StateHasChanged();
            }
        }
    }

    // Labels longer than the usual class codes (e.g. "Thera") need a smaller font to fit the small badge.
    private string BadgeStyle
    {
        get
        {
            string style = $"color:{_systemColor}; border-color:{_systemColor}; font-weight:bold;";

            if (Node.SystemType.ToDescriptionString().Length > MAX_BADGE_LABEL_LENGTH)
                style += $" font-size:{LONG_BADGE_LABEL_FONT_SIZE};";

            return style;
        }
    }

    private String systemStyle
    {
        get
        {
            String systemStyle = "width:160px; height:85px;";

            if (Node.Selected)
            {
                systemStyle += " box-shadow: 0px 0px 12px #fff;";
            }

            systemStyle += " border-color:" + WHColorHelper.GetNodeStatusColor(Node.SystemStatus) + ";";

            if (Node.IsRouteWaypoint)
            {
                systemStyle += "border-style:dashed;border-color:yellow;";
            }
            else
            {
                systemStyle += "border-style:solid;";
            }

            return systemStyle;
        }
    }

    protected override Task OnParametersSetAsync()
    {
        if (Node != null)
        {
            _systemColor = WHColorHelper.GetSystemTypeColor(Node.SystemType);
            _whEffectColor = WHColorHelper.GetEffectColor(Node.Effect);

            Locked = Node.Locked;
        }

        return base.OnParametersSetAsync();
    }

    private void StartEditingName()
    {
        _isEditingName = true;
        if (Node.AlternateName != null)
        {
            _editedName = Node.AlternateName;
        }
        else
        {
            _editedName = Node.Name;
        }
        
    }

    private async Task SaveName()
    {
        if (string.IsNullOrWhiteSpace(_editedName) || (_editedName == Node.Name))
        {
            Node.SetAlternateName(null);
        }
        else if (_editedName != Node.Name)
        {
            Node.SetAlternateName(_editedName);
        }

        _isEditingName = false;
        await InvokeAsync(StateHasChanged);
    }
    
        private async Task OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SaveName();
        }
        else if (e.Key == "Escape")
        {
            _isEditingName = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}


