using System;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Pages.Mapper.Signatures
{
    public partial class Overview : ComponentBase
    {
        [Inject]
        IWHSignature DbWHSystems { get; set; }

        private IEnumerable<WHSignature> Signatures { get; set; } 

        [Parameter]
        public EveSystemNodeModel? CurrentSystemNode { get; set; }


        protected override Task OnInitializedAsync()
        {
            Signatures = new List<WHSignature>();

            return base.OnInitializedAsync();
        }


        protected override async Task OnParametersSetAsync()
        {
            if(CurrentSystemNode!=null)
            {
                Signatures = (await DbWHSystems.GetByName(CurrentSystemNode?.Name)).WHSignatures.ToList();
            }
        }

    }
}

