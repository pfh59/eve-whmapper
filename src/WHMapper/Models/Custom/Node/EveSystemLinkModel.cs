using Blazor.Diagrams.Core.Models;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Custom.Node
{

    public class EveSystemLinkModel : LinkModel
    {
        private WHSystemLink _whLink;
        private LinkLabelModel? _labelSize=null;

        public new int Id
        {
            get
            {
                return _whLink.Id;
            }
        }


        public SystemLinkEOLStatus EndOfLifeStatus
        {
            get
            {
                return _whLink.EndOfLifeStatus;
            }
            set
            {
                _whLink.EndOfLifeStatus = value;
            }
        }

        public bool IsEoL
        {
            get
            {
                return _whLink.EndOfLifeStatus != SystemLinkEOLStatus.Normal;
            }
            set
            {
                // Legacy support: convert bool to enum
                _whLink.EndOfLifeStatus = value ? SystemLinkEOLStatus.EOL4h : SystemLinkEOLStatus.Normal;
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

        private Task SetLabel(SystemLinkSize size)
        {
            this.Labels.Clear();
            this._labelSize=null;
            switch (size)
            {
                case SystemLinkSize.Small:
                    _labelSize = new LinkLabelModel(this, "S");
                    this.Labels.Add(_labelSize);
                    break;
                case SystemLinkSize.Medium:
                    _labelSize = new LinkLabelModel(this, "M");
                    this.Labels.Add(_labelSize);
                    break;
                case SystemLinkSize.XLarge:
                    _labelSize = new LinkLabelModel(this, "XL");
                    this.Labels.Add(_labelSize);
                    break;
                
            }

            return Task.CompletedTask;
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

