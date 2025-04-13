using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;

namespace WHMapper.Components.Pages.Mapper.CustomNode;

public partial class EveSystemLink
{
    private const string DEFAULT_COLOR = IWHColorHelper.DEFAULT_COLOR;
    private string? _eolColor;
    

    [Inject]
    private IWHColorHelper? WHColorHelper { get; set; }


    [Parameter]
    public EveSystemLinkModel Link {get;set;}=null!;

    protected override Task OnParametersSetAsync()
    {
        if (Link != null)
        {
            _eolColor = WHColorHelper?.GetLinkEOLColor();
        }
        return base.OnParametersSetAsync();
    }       
}


