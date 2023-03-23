using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Pages.Mapper.Signatures
{
    public partial class Delete : Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string MSG_DELETE_SIGNATURE = "Do you really want to delete these signature?";
        private const string MSG_DELETE_SIGNATURES = "Do you really want to delete all signatures?";

        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        IWHSystemRepository? DbWHSystems { get; set; }

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        private bool _success = false;

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
            if (CurrentSystemNodeId > 0 && SignatureId > 0)
            {
                var res = await DbWHSystems.RemoveWHSignature(CurrentSystemNodeId, SignatureId);
                if (res != null && res.Id == SignatureId)
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
            if (CurrentSystemNodeId > 0)
            {
                if(await DbWHSystems.RemoveAllWHSignature(CurrentSystemNodeId))
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

        private async Task Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

