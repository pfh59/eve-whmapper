using System;
namespace WHMapper.Services.EveOnlineUserInfosProvider
{
	public interface IEveUserInfosServices
	{
		public Task<string> GetUserName();
		public Task<string> GetCharactedID();
    }
}

