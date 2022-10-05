using System;
namespace WHMapper.Services.WHColor
{
    public interface IWHColorHelper
    {
        string GetSecurityStatusColor(float secStatus);
        string GetSystemTypeColor(string systemType);
        string GetEffectColor(string effectName);
    }
}

