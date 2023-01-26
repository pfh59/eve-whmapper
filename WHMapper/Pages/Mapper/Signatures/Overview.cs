using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystems;
using static MudBlazor.CategoryTypes;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;

namespace WHMapper.Pages.Mapper.Signatures
{
    public partial class Overview : ComponentBase
    {
        [Inject]
        IWHSystemRepository? DbWHSystems { get; set; }

        [Inject]
        IWHSignatureRepository? DbWHSignatures { get; set; }

        [Inject]
        IDialogService? DialogService { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        private IEnumerable<WHSignature>? Signatures { get; set; }

        [Parameter]
        public EveSystemNodeModel? CurrentSystemNode { get; set; }

        private int? _currentSystemNodeId = 0;

        private WHSignature _selectedSignature = null;
        private WHSignature _signatureBeforeEdit;

        private bool _isEditingSignature = false;


 
        protected override async Task OnParametersSetAsync()
        {
            await Restore();
        }

        private string GetDisplayText(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Type type = value.GetType();
            if (Attribute.IsDefined(type, typeof(FlagsAttribute)))
            {
                var sb = new System.Text.StringBuilder();

                foreach (Enum field in Enum.GetValues(type))
                {
                    if (Convert.ToInt64(field) == 0 && Convert.ToInt32(value) > 0)
                        continue;

                    if (value.HasFlag(field))
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        var f = type.GetField(field.ToString());
                        var da = (DisplayAttribute)Attribute.GetCustomAttribute(f, typeof(DisplayAttribute));
                        sb.Append(da?.ShortName ?? da?.Name ?? field.ToString());
                    }
                }

                return sb.ToString();
            }
            else
            {
                var f = type.GetField(value.ToString());
                if (f != null)
                {
                    var da = (DisplayAttribute)Attribute.GetCustomAttribute(f, typeof(DisplayAttribute));
                    if (da != null)
                        return da.ShortName ?? da.Name;
                }
            }

            return value.ToString();
        }

        private async Task  OpenImportDialog()
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                DisableBackdropClick = true,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", _currentSystemNodeId);

            var dialog = DialogService?.Show<Import>("Import Scan Dialog", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Cancelled)
                await Restore();
            
        }

        private async Task Restore()
        {
            if (CurrentSystemNode != null)
            {

                var currentSystem = await DbWHSystems?.GetByName(CurrentSystemNode?.Name);
                _currentSystemNodeId = currentSystem?.Id;

                Signatures = currentSystem?.WHSignatures.ToList();
            }
            else
            {
                Signatures = null;
            }
        }

        private async Task DeleteSignature(int id)
        {
            var parameters = new DialogParameters();
            parameters.Add("CurrentSystemNodeId", _currentSystemNodeId);
            parameters.Add("SignatureId", id);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService?.Show<Delete>("Delete", parameters, options);
            DialogResult result = await dialog.Result;

            if (!result.Cancelled)
                await Restore();
        }

        private void BackupSingature(object element)
        {
            _isEditingSignature = true;

            _signatureBeforeEdit = new WHSignature(
                ((WHSignature)element).Name,
                ((WHSignature)element).Group,
                ((WHSignature)element).Type
                );

            StateHasChanged();
        }

        private void ResetSingatureToOriginalValues(object element)
        {
           

            ((WHSignature)element).Name = _signatureBeforeEdit.Name;
            ((WHSignature)element).Group = _signatureBeforeEdit.Group;
            ((WHSignature)element).Type = _signatureBeforeEdit.Type;

            _isEditingSignature = false;
            StateHasChanged();
        }

        private async void SignatiureHasBeenCommitted(object element)
        {
            var res = await DbWHSignatures.Update(((WHSignature)element).Id, ((WHSignature)element));

            if(res!=null && res.Id== ((WHSignature)element).Id)
                Snackbar.Add("Signature successfully updated", Severity.Success);
            else
                Snackbar.Add("No signature updated", Severity.Error);

            _isEditingSignature = false;
            StateHasChanged();
        }
    }
}

