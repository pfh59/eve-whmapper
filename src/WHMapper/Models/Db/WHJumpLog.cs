using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

public class WHJumpLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int WHSystemLinkId { get; set; }

    [Required]
    public int CharacterId { get; set; }
    
    [Required]
    public DateTime JumpDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int ShipTypeId { get; set; }
    [Required]
    public long ShipItemId { get; set; }

    [Required]
    public float ShipMass { get; set; }

    public WHJumpLog()
    {
        
    }
    public WHJumpLog(int linkId,int characterId,int shipTypeId, long shipItemId, float shipMass)
    {
        WHSystemLinkId=linkId;
        CharacterId = characterId;
        JumpDate = DateTime.UtcNow;
        ShipTypeId = shipTypeId;
        ShipItemId = shipItemId;
        ShipMass = shipMass;
    }

}
