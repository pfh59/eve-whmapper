using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Services.EveAPI;

namespace WHMapper.Pages.Mapper.LinkInfos;

[Authorize(Policy = "Access")]
public partial class Overview : ComponentBase
{
        [Inject]
        private ILogger<Overview> Logger { get; set; } = null!;
        [Inject]
        private IEveAPIServices EveAPIServices { get; set; } = null!;
        [Inject]
        private IWHSystemLinkRepository DbSystemLink { get; set; } = null!;

        [Parameter]
        public EveSystemLinkModel CurrentSystemLink {get;set;}= null!;

        private string FirstJumpLogCharacterName { get; set; } = string.Empty;
        private DateTime? FirstJumpLogDate { get; set; } = null;

        override protected Task OnParametersSetAsync()
        {
            Task.WhenAll(Task.Run(() => Restore()));
            return base.OnParametersSetAsync();
        }

        private async Task Restore()
        {
                if (CurrentSystemLink != null)
                {
                        var currentSystemLink = await DbSystemLink.GetById(CurrentSystemLink.Id);
                        if (currentSystemLink != null)
                        {
                                var firstJump = currentSystemLink.JumpHistory.FirstOrDefault();
                                if(firstJump!=null)
                                {
                                        FirstJumpLogCharacterName = await GetJumpLogCharacterName(firstJump);
                                        FirstJumpLogDate = firstJump.JumpDate;

                                        await InvokeAsync(() => {
                                                StateHasChanged();
                                        });
                                }
                        }
                }
        }


        private async Task<string> GetJumpLogCharacterName(WHJumpLog jumplog)
        {
                if (jumplog == null)
                {
                        Logger.LogError("Jumplog is null");
                        return string.Empty;
                }

                var character = await EveAPIServices.CharacterServices.GetCharacter(jumplog.CharacterId);

                if (character == null)
                {
                        Logger.LogError("Character is null");
                        return string.Empty;
                }

                return character.Name;
        }
}




