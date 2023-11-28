using System;
using System.ComponentModel.DataAnnotations;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Extensions;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using static MudBlazor.CategoryTypes;
using WHMapper.Services.WHColor;

namespace WHMapper.Models.Custom.Node
{

    public class EveSystemLinkModel : LinkModel
    {
        private WHSystemLink _whLink;

        public int Id
        {
            get
            {
                return _whLink.Id;

            }
        }


        public bool IsEoL
        {
            get
            {
                return _whLink.IsEndOfLifeConnection;
            }
            set
            {

                _whLink.IsEndOfLifeConnection = value;
            }
        }

        public SystemLinkSize Size
        {
            get
            {
                return _whLink.Size;
            }
            set
            {
                _whLink.Size = value;
                SetLabel(_whLink.Size);
            }
        }


        public SystemLinkMassStatus MassStatus
        {
            get
            {
                return _whLink.MassStatus;
            }
            set
            {
                _whLink.MassStatus = value;
            }
        }

        private void SetLabel(SystemLinkSize size)
        {
            this.Labels.Clear();
            switch (size)
            {
                case SystemLinkSize.Small:
                    this.Labels.Add(new LinkLabelModel(this, "S"));
                    break;
                case SystemLinkSize.Medium:
                    this.Labels.Add(new LinkLabelModel(this, "M"));
                    break;
                case SystemLinkSize.XLarge:
                    this.Labels.Add(new LinkLabelModel(this, "XL"));
                    break;
            }
        }

        public EveSystemLinkModel(WHSystemLink whLink,EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort)
            : base (sourcePort, targetPort)
        {
            _whLink = whLink;
            SetLabel(_whLink.Size);

        }

    }
}

