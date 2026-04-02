using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;

namespace WHMapper.Services.EveMapper;

public interface IInstanceRegistrationHelper
{
    Task<InstanceRegistrationContext> LoadRegistrationContextAsync(string? clientId);

    Task<WHInstance?> RegisterInstanceAsync(
        InstanceRegistrationContext context,
        string instanceName,
        string? description,
        WHAccessEntity ownerType);
}
