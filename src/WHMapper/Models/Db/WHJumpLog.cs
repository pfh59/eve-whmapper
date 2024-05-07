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
    public DateTime JumpDate { get; set; }

    public int? ShipTypeId { get; set; }

    public long? ShipItemId { get; set; }
    
    public float? ShipMass { get; set; }

    [Obsolete("EF Requires it")]
    protected WHJumpLog() { }
    
    //Construstor when lin k are create manually, no ship used
    public WHJumpLog(int linkId,int characterId) :
     this(linkId, characterId, null, null, null)
    {

    }

   //Constrtuctor for real jump with a real ship 
    public WHJumpLog(int linkId,int characterId,int? shipTypeId, long? shipItemId, float? shipMass)
    {
        WHSystemLinkId=linkId;
        CharacterId = characterId;
        JumpDate = DateTime.UtcNow;
        ShipTypeId = shipTypeId;
        ShipItemId = shipItemId;
        ShipMass = shipMass;
    }

}
