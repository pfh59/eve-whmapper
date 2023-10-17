using System;
namespace WHMapper.Services.EveOnlineUserInfosProvider
{
	public interface IEveUserInfosServices
	{
        public const string ANONYMOUS_USERNAME = "Anonymous";

        public Task<string> GetUserName();
		public Task<string> GetCharactedID();
    }
}

