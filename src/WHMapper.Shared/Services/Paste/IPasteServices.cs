namespace WHMapper.Shared.Services.Paste;

public interface IPasteServices
{
    event Func<string?, Task> Pasted;
    Task Paste(string? value);
}
