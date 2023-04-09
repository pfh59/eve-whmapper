using System;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;
using FluentValidation;
using static MudBlazor.CategoryTypes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Authorization;
using Severity = MudBlazor.Severity;
using Microsoft.AspNetCore.Hosting.Server;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.WHSignature;
using System.Diagnostics.Tracing;
using System.Reflection.Emit;

namespace WHMapper.Pages.Mapper.Signatures
{

    public partial class Import : Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string _scanResultRegex = "[a-zA-Z]{3}-[0-9]{3}\\s([a-zA-Z\\s]+)[0-9]*.[0-9]+%\\s[0-9]*.[0-9]+\\sAU";


        [Inject]
        private IEveUserInfosServices UserService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IWHSignatureHelper? SignatureHelper { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public int CurrentSystemNodeId { get; set; }


        private MudForm _form;
        private bool _success = false;
        private FluentValueValidator<string> _ccValidator = new FluentValueValidator<string>(x => x
            .NotEmpty()
            .NotNull()
            .Matches(_scanResultRegex));

        private string _scanResult = String.Empty;

        

        protected override Task OnInitializedAsync()
        {
        
            return base.OnInitializedAsync();
        }



        private async Task Submit()
        {
            await _form.Validate();

            if (_form.IsValid)
            {
                try
                {
                    String scanUser = await UserService.GetUserName();
                    if (await SignatureHelper?.ImportScanResult(scanUser, CurrentSystemNodeId, _scanResult))
                    {
                        Snackbar?.Add("Signatures successfully added/updated", Severity.Success);
                        MudDialog.Close(DialogResult.Ok(true));
                    }
                    else
                    {
                        Snackbar?.Add("No signatures added/updated", Severity.Error);
                        MudDialog.Close(DialogResult.Ok(false));
                    }
                }
                catch (Exception ex)
                {
                    Snackbar?.Add(ex.Message, Severity.Error);
                    MudDialog.Close(DialogResult.Ok(false));
                }
            }
            else
            {
                Snackbar?.Add("Bad signatures format", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }

        private async Task Cancel()
        {
            MudDialog.Cancel();
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

