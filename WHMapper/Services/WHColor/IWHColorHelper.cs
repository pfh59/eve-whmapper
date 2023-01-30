using System;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.WHColor
{
    public interface IWHColorHelper
    {
        string GetSecurityStatusColor(float secStatus);
        string GetSystemTypeColor(string systemType);
        string GetEffectColor(string effectName);

        string GetLinkEOLColor();
        string GetLinkStatusColor(SystemLinkMassStatus status);
        string GetLinkSelectedColor();
    }
}

