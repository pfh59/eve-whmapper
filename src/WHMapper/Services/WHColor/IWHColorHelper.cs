using System;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.WHColor
{
    public interface IWHColorHelper
    { 
        const string DEFAULT_COLOR = "grey";

        string GetSecurityStatusColor(float secStatus);
        string GetSystemTypeColor(EveSystemType systemType);
        string GetEffectColor(WHEffect effect);

        string GetLinkEOLColor();
        string GetLinkStatusColor(SystemLinkMassStatus status);
        string GetLinkSelectedColor();

        string GetNodeStatusColor(WHSystemStatusEnum status);
    }
}

