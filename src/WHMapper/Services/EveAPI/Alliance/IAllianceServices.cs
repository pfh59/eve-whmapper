namespace WHMapper.Services.EveAPI.Alliance
{
    public interface IAllianceServices
	{
        Task<int[]?> GetAlliances();
        Task<Models.DTO.EveAPI.Alliance.Alliance?> GetAlliance(int alliance_id);
    }
}

