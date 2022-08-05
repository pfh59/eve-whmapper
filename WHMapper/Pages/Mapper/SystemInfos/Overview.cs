using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;

namespace WHMapper.Pages.Mapper.SystemInfos
{
    public partial class Overview : ComponentBase
    {
        [Parameter]
        public EveSystemNodeModel CurrentSystemNode { get; set; }


    }
}

