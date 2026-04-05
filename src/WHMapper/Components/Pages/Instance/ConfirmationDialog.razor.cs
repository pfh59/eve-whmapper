using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WHMapper.Components.Pages.Instance;

public partial class ConfirmationDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string ContentText { get; set; } = string.Empty;

    [Parameter]
    public string ConfirmText { get; set; } = "OK";

    [Parameter]
    public string CancelText { get; set; } = "Cancel";

    [Parameter]
    public Color ButtonColor { get; set; } = Color.Primary;

    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
    private void Cancel() => MudDialog.Cancel();
}
