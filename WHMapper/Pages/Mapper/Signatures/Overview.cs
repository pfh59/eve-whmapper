using System;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Db;

namespace WHMapper.Pages.Mapper.Signatures
{
    public partial class Overview : ComponentBase
    {
        [Parameter]
        public ICollection<WHSignature> Signatures { get; set; }
    }
}

