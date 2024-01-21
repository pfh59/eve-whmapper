namespace WHMapper.Models.DTO.SDE;


public class SolarSystemJump
{
    public SolarSystem System { get;  set; }
    public  IEnumerable<SolarSystem> JumpList { get;  set; }

    public SolarSystemJump()
    {

    }
    public SolarSystemJump(int solarSystemId,float security) : this(solarSystemId, security,new List<SolarSystem>())
    {

    }

     public SolarSystemJump(int solarSystemId,float security, IEnumerable<SolarSystem> jumpList)
    {
        System=new SolarSystem(solarSystemId,security);
        JumpList = jumpList;
    }
}