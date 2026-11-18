namespace WHMapper.Models.DTO.Intel;

public enum WHIntelAiStatus
{
    Generated,

    /// <summary>No AI provider is configured.</summary>
    NotConfigured,

    /// <summary>Too few kills to ground a report; no AI call is made.</summary>
    NotEnoughData,

    /// <summary>The AI call failed or was refused.</summary>
    Unavailable
}
