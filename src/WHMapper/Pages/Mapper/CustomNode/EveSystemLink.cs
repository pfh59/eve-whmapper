using System;
using System.Xml.Linq;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
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
        public string? _eolColor;

        [Inject]
        public IWHColorHelper? WHColorHelper { get; set; }

        private EveSystemLinkModel _link;
        [Parameter]
        public EveSystemLinkModel Link
        {
            get
            {
                return _link;
            }
            set
            {
                _link = value;
                if (_link != null)
                {
                    _eolColor = WHColorHelper?.GetLinkEOLColor();
                }
            }
        }
    }
}

