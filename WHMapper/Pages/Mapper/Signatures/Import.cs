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

namespace WHMapper.Pages.Mapper.Signatures
{

    public partial class Import : Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string _scanResultRegex = "[a-zA-Z]{3}-[0-9]{3}\\s([a-zA-Z\\s]+)[0-9]*.[0-9]+%\\s[0-9]*.[0-9]+\\sAU";


        [Inject]
        public IEveUserInfosServices UserService { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        IWHSystemRepository? DbWHSystems { get; set; }

        [Inject]
        IWHSignatureRepository? DbWHSignatures { get; set; }

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
                var sigs = await ParseScanResult();
                if(sigs!=null && sigs.Count()>0)
                {
                    var currentSystem = await DbWHSystems.GetById(CurrentSystemNodeId);

                    if(currentSystem?.WHSignatures.Count==0)
                    {
                        var res = await DbWHSystems.AddWHSignatures(CurrentSystemNodeId, sigs);
                        if (res != null && res.Count() == sigs.Count())
                        {
                            Snackbar.Add("Signatures successfully added", Severity.Success);
                            MudDialog.Close(DialogResult.Ok(true));
                        }
                        else
                        {
                            Snackbar.Add("No signatures added", Severity.Error);
                            MudDialog.Close(DialogResult.Ok(false));
                        }
                    }
                    else
                    {
                        bool sigUpdated = true;
                        bool sigAdded = true;

                        var sigsToUpdate = currentSystem?.WHSignatures.IntersectBy(sigs.Select(x => x.Name), y => y.Name);
                        if (sigsToUpdate != null && sigsToUpdate.Count()>0)
                        {
                            foreach (var sig in sigsToUpdate)
                            {
                                var sigParse = sigs.Where(x => x.Name == sig.Name).FirstOrDefault();
                                if (sigParse.Group != WHSignatureGroup.Unknow)
                                {
                                    sig.Group = sigParse.Group;
                                    sig.Type = sigParse.Type;
                                }
                                
                                sig.Updated = sigParse.Updated;
                                sig.UpdatedBy = sigParse.UpdatedBy;
                            }
                            var resUpdate = await DbWHSignatures.Update(sigsToUpdate);
                            if (resUpdate != null && resUpdate.Count() == sigsToUpdate.Count())
                            {
                                sigUpdated = true;
                                Snackbar.Add("Signatures successfully updated", Severity.Success);
                            }
                            else
                            {
                                sigUpdated = false;
                                Snackbar.Add("No signatures updated", Severity.Error);
                            }
                        }


                        var sigsToAdd = sigs.ExceptBy(currentSystem?.WHSignatures.Select(x => x.Name), y => y.Name);
                        if (sigsToAdd != null && sigsToAdd.Count() > 0)
                        {
                            var resAdd = await DbWHSystems.AddWHSignatures(CurrentSystemNodeId, sigsToAdd);
                            if (resAdd != null && resAdd.Count() == sigsToAdd.Count())
                            {
                                sigAdded = true;
                                Snackbar.Add("Signatures successfully added", Severity.Success);
                            }
                            else
                            {
                                sigAdded = false;
                                Snackbar.Add("No signatures added", Severity.Error);
                            }
                        }

                        if (sigUpdated || sigAdded)
                        {
                            
                            MudDialog.Close(DialogResult.Ok(true));
                        }
                        else 
                        {
                            MudDialog.Close(DialogResult.Ok(false));
                        }
                    }
                }
                else
                {
                    Snackbar.Add("Bad signature parsing parameters", Severity.Error);
                    MudDialog.Close(DialogResult.Ok(false));
                }
            }
        }

        private async Task<IEnumerable<WHSignature>> ParseScanResult()
        {
            IList<WHSignature> sigResult = new List<WHSignature>();

            Regex lineRegex = new Regex("\n");
            Regex tabRegex = new Regex("\t");
            string[] sigvalues = lineRegex.Split(_scanResult);

            string scanUser = await UserService.GetUserName();

            foreach (string sigValue in sigvalues)
            {
                var splittedSig = tabRegex.Split(sigValue);
                WHSignature newSig = new WHSignature(splittedSig[0], scanUser);

                if (!String.IsNullOrWhiteSpace(splittedSig[2]))
                {
                    WHSignatureGroup group = WHSignatureGroup.Unknow;

                    var sigGroup = splittedSig[2];
                    if (splittedSig[2].Contains(' '))
                        sigGroup = splittedSig[2].Split(' ').First();

                    if (Enum.TryParse<WHSignatureGroup>(sigGroup, out group))
                        newSig.Group = group;
                }
                 

                sigResult.Add(newSig);
            }

            return sigResult;

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

