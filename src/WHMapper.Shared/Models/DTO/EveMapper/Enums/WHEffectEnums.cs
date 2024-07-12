using System.ComponentModel;

namespace WHMapper.Shared.Models.DTO.EveMapper.Enums
{
    public enum WHEffect
    {
        [Description("Magnetar")]
        Magnetar,
        [Description("Red Giant")]
        RedGiant,
        [Description("Pulsar")]
        Pulsar,
        [Description("Wolf-Rayet Star")]
        WolfRayet,
        [Description("Cataclysmic Variable")]
        Cataclysmic,
        [Description("Black Hole")]
        BlackHole,
        None
    }
}

