using System;
using System.Reflection.Emit;
using System.Xml.Linq;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Models.Custom.Node;
using WHMapper.Services.WHColor;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemLink : ComponentBase
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
}

