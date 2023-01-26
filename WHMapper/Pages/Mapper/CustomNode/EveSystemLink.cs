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
                
            }
        }
    }
}

