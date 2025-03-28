using System;

namespace WHMapper.Models.DTO.EveMapper;

public class WHMapperUser
{
    public int Id { get; private set; }
    public string PortraitUrl { get; private set; }
    public bool Tracking { get; set; }
    public bool IsPrimary { get; set; }

    public WHMapperUser(int id, string portraitUrl, bool tracking = true)
    {
        Id = id;
        PortraitUrl = portraitUrl;
        Tracking = tracking;
    }   

}
