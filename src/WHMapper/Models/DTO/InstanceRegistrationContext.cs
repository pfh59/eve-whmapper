using WHMapper.Models.DTO.EveAPI.Character;

namespace WHMapper.Models.DTO;

public class InstanceRegistrationContext
{
    public bool IsAuthenticated { get; set; }
    public bool AlreadyHasInstance { get; set; }
    public int ExistingInstanceId { get; set; }

    public int CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Character? CharacterInfo { get; set; }
    public string CorporationName { get; set; } = string.Empty;
    public string AllianceName { get; set; } = string.Empty;
}
