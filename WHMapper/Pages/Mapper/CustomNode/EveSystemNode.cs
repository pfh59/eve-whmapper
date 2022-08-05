using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;

namespace WHMapper.Pages.Mapper.CustomNode
{
    public partial class EveSystemNode : ComponentBase
    {
        [ParameterAttribute]
        public EveSystemNodeModel Node { get; set; }
    }
}

