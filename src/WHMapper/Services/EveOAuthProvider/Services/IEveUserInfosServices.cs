namespace WHMapper.Services.EveOAuthProvider.Services;

public interface IEveUserInfosServices
{
    public const string ANONYMOUS_USERNAME = "Anonymous";
    Task<string> GetUserName();
    Task<int> GetCharactedID();
}

