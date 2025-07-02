using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;


namespace WHMapper.Components.Pages.Mapper.CustomNode;

public partial class EveSystemNode
{
    private string _secColor = IWHColorHelper.DEFAULT_COLOR;
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

    private String systemStyle
    {
        get
        {
            String systemStyle = "";

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
            _secColor = WHColorHelper.GetSecurityStatusColor(Node.SecurityStatus);
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


