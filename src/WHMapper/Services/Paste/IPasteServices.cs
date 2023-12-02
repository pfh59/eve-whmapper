namespace WHMapper;

public interface IPasteServices
{
    event Func<string?, Task> Pasted;
    Task Paste(string? value);
}
