using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using FluentValidation;
using Severity = MudBlazor.Severity;
using WHMapper.Services.WHSignature;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.WHColor;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Components.Pages.Mapper.Signatures;

[Authorize(Policy = "Access")]
public partial class Import
{

    [Inject]
    private IEveUserInfosServices  UserService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHSignatureHelper SignatureHelper { get; set; } = null!;

    [Inject]
    private IWHColorHelper ColorHelper { get; set; } = null!;

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int CurrentSystemNodeId { get; set; }

    private IEnumerable<WHSignature>? _currentSystemSigs = null!;
    private string scanUser = String.Empty;

    private MudForm _form = null!;
    private bool _success = false;
    private bool Success
    {
        get => _success;
        set
        {
            _success = value;  
            if(_success)
            {
                Task.Run(()=>Analyze());  
            } 
            else
            {
                AnalyzesSignatures = Enumerable.Empty<WHAnalizedSignature>();
            } 
        }
    }

    private IEnumerable<WHAnalizedSignature>? AnalyzesSignatures { get; set; } = null!;


    private FluentSignatureValueValidator<string> _ccValidator =null!;


    private string _scanResult = string.Empty;
    
    private string ScanResult
    { 
        get
        {
            return _scanResult;
        }
        set
        {
            Success = false;
            _scanResult = value;
        }
    }
    private bool _lazyDeleted = false;

    private bool LazyDeleted 
    {
        get => _lazyDeleted;
        set
        {
            _lazyDeleted = value;
            Task.Run(()=>Analyze());
        }
    }

    protected override void OnInitialized()
    {
            _ccValidator  = new FluentSignatureValueValidator<string>(x => x
            .NotEmpty()
            .NotNull()
            .MustAsync(async (context, value, cancellationToken) => await SignatureHelper.ValidateScanResult(value))
        );
    }

    protected override Task OnParametersSetAsync()
    {
        Task.Run(() => Restore());
        return base.OnParametersSetAsync();
    }

    private async Task Restore()
    {

        ScanResult = String.Empty;
        _lazyDeleted = false;
        Success = false;
        _currentSystemSigs = await SignatureHelper.GetCurrentSystemSignatures(CurrentSystemNodeId);
        scanUser = await UserService.GetUserName();
    }

    private async Task Submit()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            try
            {
                if (await SignatureHelper.ImportScanResult(scanUser, CurrentSystemNodeId, ScanResult, _lazyDeleted))
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

    private async Task Analyze()
    {
        var sigs = await SignatureHelper.ParseScanResult(scanUser, CurrentSystemNodeId, ScanResult);
        var res = await SignatureHelper.AnalyzedSignatures(sigs, _currentSystemSigs, _lazyDeleted);
        AnalyzesSignatures = res?.OrderBy(x=>x.Name);
        await InvokeAsync(() => {
            StateHasChanged();
        });
    }

    private string RowStyleFunc(WHAnalizedSignature item, int index)
    {
        return "background-color:"+ColorHelper.GetWHAnalyzedSignatureColor(item.Status);
    }
}

/// <summary>
/// A glue class to make it easy to define validation rules for single values using FluentValidation
/// You can reuse this class for all your fields, like for the credit card rules above.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class FluentSignatureValueValidator<T> : AbstractValidator<T>
{
    public FluentSignatureValueValidator(Action<IRuleBuilderInitial<T, T>> rule)
    {
        rule(RuleFor(x => x));
    }

    private async Task<IEnumerable<string>> ValidateValue(T arg)
    {
        var result = await ValidateAsync(arg);
        if (result.IsValid)
            return new string[0];
        return result.Errors.Select(e => e.ErrorMessage);
    }

    public Func<T, Task<IEnumerable<string>>> Validation => async item => await ValidateValue(item);
}



