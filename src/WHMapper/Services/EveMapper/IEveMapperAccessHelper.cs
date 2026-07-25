namespace WHMapper.Services.EveMapper
{
    public interface IEveMapperAccessHelper
    {
        public Task<bool> IsEveMapperUserAccessAuthorized(int eveCharacterId);
        public Task<bool> IsEveMapperUserAccessAuthorizedForAny(IEnumerable<int> eveCharacterIds);
        public Task<bool> IsEveMapperAdminAccessAuthorized(int eveCharacterId);
        public Task<bool> IsEveMapperMapAccessAuthorized(int eveCharacterId, int mapId);
        public Task<bool> IsEveMapperInstanceAccessAuthorized(int eveCharacterId, int instanceId);

        /// <summary>
        /// Ids of every instance the character can access, through character, corporation or alliance.
        /// </summary>
        public Task<IEnumerable<int>> GetAccessibleInstanceIdsAsync(int eveCharacterId);
        public Task<bool> IsInstanceAdminAuthorized(int eveCharacterId, int instanceId);

        /// <summary>
        /// Returns the instance a map belongs to, or null when the map is unknown or orphaned.
        /// </summary>
        public Task<int?> GetMapInstanceIdAsync(int mapId);
    }
}
