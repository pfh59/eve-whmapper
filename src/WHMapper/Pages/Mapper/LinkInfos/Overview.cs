using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        private WHSystemLink? SystemLink{get;set;}=null!;

        private string FirstJumpLogCharacterName { get; set; } = string.Empty;
        private string FirstJumpLogShipName { get; set; } = string.Empty;
        private DateTime? FirstJumpLogDate { get; set; } = null;

        private string LastJumpLogCharacterName{ get; set; } = string.Empty;
        private string LastJumpLogShipName { get; set; } = string.Empty;
        private DateTime? LastJumpLogDate { get; set; } = null;

        private bool isLoading = true;
        private bool showing = false;

        override protected Task OnParametersSetAsync()
        {
            Task.WhenAll(Task.Run(() => Restore()));
            return base.OnParametersSetAsync();
        }

        private async Task Restore()
        {        
                try
                {       
                        isLoading=true; 
                        showing=false;
                        FirstJumpLogCharacterName = string.Empty;
                        FirstJumpLogShipName = string.Empty;
                        FirstJumpLogDate = null;
                        LastJumpLogCharacterName = string.Empty;
                        LastJumpLogShipName = string.Empty;
                        LastJumpLogDate = null;
                
                        if (CurrentSystemLink != null)
                        {
                                SystemLink = await DbSystemLink.GetById(CurrentSystemLink.Id);
                                if (SystemLink != null)
                                {
                                        var firstJump = SystemLink.JumpHistory.FirstOrDefault();
                                        if(firstJump!=null)
                                        {
                                                showing=true;
                                                FirstJumpLogCharacterName = await GetJumpLogCharacterName(firstJump);
                                                FirstJumpLogDate = firstJump.JumpDate;
                                                FirstJumpLogShipName = await GetJumpLogShipName(firstJump);
                                        }

                                        var lastJump = SystemLink.JumpHistory.LastOrDefault();
                                        if(lastJump!=null)
                                        {      
                                                showing=true;
                                                LastJumpLogCharacterName = await GetJumpLogCharacterName(lastJump);
                                                LastJumpLogDate = lastJump.JumpDate;
                                                LastJumpLogShipName = await GetJumpLogShipName(lastJump);

                                        }
                                }
                        }
                }
                catch (Exception ex)
                {
                        Logger.LogError(ex, "Error during restore");
                }
                finally
                {
                        isLoading=false;
                        await InvokeAsync(() => {
                                StateHasChanged();
                         });
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

        private async Task<string> GetJumpLogShipName(WHJumpLog jumplog)
        {
                if (jumplog == null)
                {
                        Logger.LogError("Jumplog is null");
                        return string.Empty;
                }

                var shipInfos = await EveAPIServices.UniverseServices.GetType(jumplog!.ShipTypeId);

                if (shipInfos == null)
                {
                        Logger.LogError("Ship is null");
                        return string.Empty;
                }

                return shipInfos.Name;
        }
}




