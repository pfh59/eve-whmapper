using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Shared.Repositories.WHSignatures;

namespace WHMapper.Pages.Mapper.Signatures
{
    [Authorize(Policy = "Access")]
    public partial class Delete : Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string MSG_DELETE_SIGNATURE = "Do you really want to delete these signature?";
        private const string MSG_DELETE_SIGNATURES = "Do you really want to delete all signatures?";

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;
        [Inject]
        IWHSignatureRepository DbWHSignatures { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int CurrentSystemNodeId { get; set; }

        [Parameter]
        public int SignatureId { get; set; }
        

        private async Task Submit()
        {
            if(CurrentSystemNodeId>0)
            {
                if (SignatureId > 0)
                    await DeleteSignature();
                else
                    await DeleteSignatures();
            }
 
        }

        private async Task DeleteSignature()
        {
            if(DbWHSignatures==null)
            {
                Snackbar.Add("DbWHSignatures is null", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
                

            if (CurrentSystemNodeId > 0 && SignatureId > 0)
            {
                if (DbWHSignatures!=null  && await DbWHSignatures.DeleteById(SignatureId))
                {
                    Snackbar.Add("Signature successfully deleted", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add("No Signature deleted", Severity.Error);
                    MudDialog.Close(DialogResult.Ok(false));
                }
            }
            else
            {
                Snackbar.Add("Bad signature parameters", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }

        private async Task DeleteSignatures()
        {
            if (DbWHSignatures == null)
            {
                Snackbar.Add("DbWHSignatures is null", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }

            if (CurrentSystemNodeId > 0)
            {
                if(DbWHSignatures!=null && await DbWHSignatures.DeleteByWHId(CurrentSystemNodeId))
                {
                    Snackbar.Add("All Signatures are successfully deleted", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add("No Signature deleted", Severity.Error);
                    MudDialog.Close(DialogResult.Ok(false));
                }
            }
            else
            {
                Snackbar.Add("Bad signature parameters", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }

        private void Cancel()
        {
            MudDialog?.Cancel();
        }
    }
}

