namespace WHMapper.Services.EveMapper
{
    public interface IEveMapperAccessHelper
    {
        public Task<bool> IsEveMapperUserAccessAuthorized(int eveCharacterId);
        public Task<bool> IsEveMapperAdminAccessAuthorized(int eveCharacterId);
    }
}
