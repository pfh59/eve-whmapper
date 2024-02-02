using System.Security;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper;

public class PasteServices : IPasteServices
{
    public event Func<string?, Task> Pasted;

    public Task Paste(string? value)
    {
        Pasted?.Invoke(value);
        return Task.CompletedTask;
    }
}

