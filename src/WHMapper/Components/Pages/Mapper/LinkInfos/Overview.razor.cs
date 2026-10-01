using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHColor;

namespace WHMapper.Components.Pages.Mapper.LinkInfos;

[Authorize(Policy = "Access")]
public partial class Overview
{
        [Inject]
        private ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IWHSystemLinkRepository DbSystemLink { get; set; } = null!;
        
        [Inject]
        private IWHColorHelper? WHColorHelper { get; set; }

        [Inject]
        private IEveMapperService EveMapperEntity { get; set; } = null!;

        [Parameter]
        public EveSystemLinkModel CurrentSystemLink {get;set;}= null!;
        private WHSystemLink? SystemLink{get;set;}=null!;

        private string FirstJumpLogCharacterName { get; set; } = string.Empty;
        private string FirstJumpLogShipName { get; set; } = string.Empty;
        private DateTime? FirstJumpLogDate { get; set; } = null;
        
        private string LastJumpLogCharacterName{ get; set; } = string.Empty;
        private string LastJumpLogShipName { get; set; } = string.Empty;
        private DateTime? LastJumpLogDate { get; set; } = null;

        private const string NO_SHIP_USED_LABEL = "No ship used";

        /// <summary>Keeps date and time columns on one line.</summary>
        private const string NO_WRAP_STYLE = "white-space: nowrap;";

        /// <summary>Caps character and ship names (up to 37 chars in EVE) so the panel never scrolls horizontally.</summary>
        private const string TRUNCATED_NAME_STYLE = "max-width: 100px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

        private bool _isLoading = true;
        private bool _showing = false;

        /// <summary>
        /// Character names of the whole jump history, resolved once per restore.
        /// </summary>
        /// <remarks>
        /// The jump log table is virtualized: resolving names from the row template would block a
        /// thread-pool thread per visible row, on every render.
        /// </remarks>
        private IReadOnlyDictionary<int, string> _characterNames = new Dictionary<int, string>();

        /// <summary>
        /// Ship names of the whole jump history, resolved once per restore.
        /// </summary>
        private IReadOnlyDictionary<int, string> _shipNames = new Dictionary<int, string>();

        override protected Task OnParametersSetAsync()
        {
            Task.WhenAll(Task.Run(() => Restore()));
            return base.OnParametersSetAsync();
        }

        private async Task Restore()
        {        
                try
                {       
                        _isLoading=true; 
                        _showing=false;
                        FirstJumpLogCharacterName = string.Empty;
                        FirstJumpLogShipName = string.Empty;
                        FirstJumpLogDate = null;
                        LastJumpLogCharacterName = string.Empty;
                        LastJumpLogShipName = string.Empty;
                        LastJumpLogDate = null;
                        _characterNames = new Dictionary<int, string>();
                        _shipNames = new Dictionary<int, string>();

                        if (CurrentSystemLink != null)
                        {
                                SystemLink = await DbSystemLink.GetById(CurrentSystemLink.Id);
                                if (SystemLink != null)
                                {
                                        var JumpHistory = SystemLink.JumpHistory.OrderBy(x => x.JumpDate).ToList();

                                        await ResolveJumpLogNamesAsync(JumpHistory);

                                        var firstJump = JumpHistory.FirstOrDefault();
                                        if(firstJump!=null)
                                        {
                                                _showing=true;
                                                FirstJumpLogCharacterName = GetJumpLogCharacterName(firstJump);
                                                FirstJumpLogDate = firstJump.JumpDate;
                                                FirstJumpLogShipName = GetJumpLogShipName(firstJump);
                                        }

                                        var lastJump = JumpHistory.LastOrDefault();
                                        if(lastJump!=null)
                                        {
                                                _showing=true;
                                                LastJumpLogCharacterName = GetJumpLogCharacterName(lastJump);
                                                LastJumpLogDate = lastJump.JumpDate;
                                                LastJumpLogShipName = GetJumpLogShipName(lastJump);

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
                        _isLoading=false;
                        await InvokeAsync(() => {
                                StateHasChanged();
                         });
                }
        }

        /// <summary>
        /// Resolves every distinct character and ship of the jump history in one pass.
        /// </summary>
        private async Task ResolveJumpLogNamesAsync(IReadOnlyCollection<WHJumpLog> jumpHistory)
        {
                var characterIds = jumpHistory.Select(x => x.CharacterId).Distinct().ToList();
                var shipTypeIds = jumpHistory
                        .Where(x => x.ShipTypeId.HasValue)
                        .Select(x => x.ShipTypeId!.Value)
                        .Distinct()
                        .ToList();

                var characters = await Task.WhenAll(characterIds.Select(EveMapperEntity.GetCharacter));
                var ships = await Task.WhenAll(shipTypeIds.Select(EveMapperEntity.GetShip));

                _characterNames = characterIds
                        .Zip(characters, (id, character) => (Id: id, character?.Name))
                        .Where(entry => !string.IsNullOrEmpty(entry.Name))
                        .ToDictionary(entry => entry.Id, entry => entry.Name!);

                _shipNames = shipTypeIds
                        .Zip(ships, (id, ship) => (Id: id, ship?.Name))
                        .Where(entry => !string.IsNullOrEmpty(entry.Name))
                        .ToDictionary(entry => entry.Id, entry => entry.Name!);
        }

        /// <summary>
        /// Gets the character name of a jump from the pre-resolved cache.
        /// </summary>
        /// <returns>Empty when the character could not be resolved.</returns>
        private string GetJumpLogCharacterName(WHJumpLog jumplog) =>
                _characterNames.GetValueOrDefault(jumplog.CharacterId, string.Empty);

        /// <summary>
        /// Gets the ship name of a jump from the pre-resolved cache.
        /// </summary>
        /// <returns>A "no ship" label when the jump carries no ship, empty when it is unresolved.</returns>
        private string GetJumpLogShipName(WHJumpLog jumplog) =>
                jumplog.ShipTypeId is null
                        ? NO_SHIP_USED_LABEL
                        : _shipNames.GetValueOrDefault(jumplog.ShipTypeId.Value, string.Empty);

        private static string GetEOLStatusLabel(SystemLinkEolStatus status)
        {
                return status switch
                {
                        SystemLinkEolStatus.Normal => "Normal",
                        SystemLinkEolStatus.EOL4h => "EOL -4h",
                        SystemLinkEolStatus.EOL1h => "EOL -1h",
                        _ => "Unknown"
                };
        }
}




