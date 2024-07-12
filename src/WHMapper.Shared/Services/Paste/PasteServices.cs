namespace WHMapper.Shared.Services.Paste;

public class PasteServices : IPasteServices
{
    public event Func<string?, Task> Pasted = null!;

    public Task Paste(string? value)
    {
        Pasted?.Invoke(value);
        return Task.CompletedTask;
    }
}
