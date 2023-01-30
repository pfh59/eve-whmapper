using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
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
        [ParameterAttribute]
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

