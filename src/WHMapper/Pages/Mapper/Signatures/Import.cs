using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Shared.Services.EveOnlineUserInfosProvider;
using WHMapper.Shared.Services.WHSignatures;
using Severity = MudBlazor.Severity;

namespace WHMapper.Pages.Mapper.Signatures
{
    [Authorize(Policy = "Access")]
    public partial class Import : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Inject]
        private IEveUserInfosServices UserService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IWHSignatureHelper SignatureHelper { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int CurrentSystemNodeId { get; set; }


        private MudForm _form = null!;
        private bool _success = false;
        private FluentValueValidator<string> _ccValidator = new FluentValueValidator<string>(x => x
            .NotEmpty()
            .NotNull()
            .Matches(IWHSignatureHelper.SCAN_VALIDATION_REGEX));

        private string _scanResult = String.Empty;
        private bool _lazyDeleted = false;
        


        private async Task Submit()
        {
            await _form.Validate();

            if (_form.IsValid)
            {
                try
                {
                    String scanUser = await UserService.GetUserName();

                    if (await SignatureHelper.ImportScanResult(scanUser, CurrentSystemNodeId, _scanResult, _lazyDeleted))
                    {
                        Snackbar.Add("Signatures successfully added/updated", Severity.Success);
                        MudDialog.Close(DialogResult.Ok(true));
                    }
                    else
                    {
                        Snackbar.Add("No signatures added/updated", Severity.Error);
                        MudDialog.Close(DialogResult.Ok(false));
                    }
                }
                catch (Exception ex)
                {
                    Snackbar.Add(ex.Message, Severity.Error);
                    MudDialog.Close(DialogResult.Ok(false));
                }
            }
            else
            {
                Snackbar.Add("Bad signatures format", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }

        private Task Cancel()
        {
            MudDialog.Cancel();
            return Task.CompletedTask;
        }

    }

    /// <summary>
    /// A glue class to make it easy to define validation rules for single values using FluentValidation
    /// You can reuse this class for all your fields, like for the credit card rules above.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FluentValueValidator<T> : AbstractValidator<T>
    {
        public FluentValueValidator(Action<IRuleBuilderInitial<T, T>> rule)
        {
            rule(RuleFor(x => x));
        }

        private IEnumerable<string> ValidateValue(T arg)
        {
            var result = Validate(arg);
            if (result.IsValid)
                return new string[0];
            return result.Errors.Select(e => e.ErrorMessage);
        }

        public Func<T, IEnumerable<string>> Validation => ValidateValue;
    }

}

