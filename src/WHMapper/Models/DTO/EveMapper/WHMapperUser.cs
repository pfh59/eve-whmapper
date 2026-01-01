using System;

namespace WHMapper.Models.DTO.EveMapper;

public class WHMapperUser
{
    public int Id { get; private set; }
    public string PortraitUrl { get; private set; }
    public bool Tracking { get; set; }
    public bool IsPrimary { get; set; }
    
    /// <summary>
    /// Indicates whether this account has access to the maps of the primary account.
    /// Secondary accounts without map access should have tracking disabled.
    /// </summary>
    public bool HasMapAccess { get; set; }
    
    /// <summary>
    /// Indicates whether this account has access to the currently selected map.
    /// If false, tracking should be disabled and cannot be enabled for this account.
    /// </summary>
    public bool HasCurrentMapAccess { get; set; }

    public WHMapperUser(int id, string portraitUrl, bool tracking = true, bool hasMapAccess = true)
    {
        Id = id;
        PortraitUrl = portraitUrl;
        Tracking = tracking;
        HasMapAccess = hasMapAccess;
        HasCurrentMapAccess = hasMapAccess; // Default to same as HasMapAccess
    }   

}
