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

        public new int Id
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

        public bool IsRouteWaypoint{get;set;} = false;

        public EveSystemLinkModel(WHSystemLink whLink,EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort)
            : base (sourcePort, targetPort)
        {
            var linkMarker = new LinkMarker("M 0 4 C 1.5 5.5 3.5 6 6 6 L 6 -6 C 3.5 -6 1.5 -5.5 0 -4 L 0 4", 6);
            SourceMarker = linkMarker;
            TargetMarker = linkMarker;
            _whLink = whLink;
            SetLabel(_whLink.Size);

        }

    }
}

