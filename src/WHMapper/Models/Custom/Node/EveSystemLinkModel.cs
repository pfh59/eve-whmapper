using System;
using System.ComponentModel.DataAnnotations;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Extensions;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using static MudBlazor.CategoryTypes;
using WHMapper.Services.WHColor;
using Blazor.Diagrams.Core.Geometry;

namespace WHMapper.Models.Custom.Node
{

    public class EveSystemLinkModel : LinkModel
    {
        private WHSystemLink _whLink;
        private LinkLabelModel? _labelSize=null;
        private LinkLabelModel? _labelFirstJump=null;

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

        public IEnumerable<WHJumpLog> JumpHistory
        {
            get
            {
                return _whLink.JumpHistory.OrderBy(x => x.JumpDate);
            }
        }

        public WHJumpLog? FirstJump
        {
            get
            {
                if(JumpHistory.Count() == 0)
                {
                    return null;
                }
                return JumpHistory.First();
            }
        }

        public WHJumpLog? LatestJump
        {
            get
            {
                if(JumpHistory.Count() == 0)
                {
                    return null;
                }
                return JumpHistory.Last();
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

            if(_labelFirstJump != null)
            {
                this.Labels.Add(_labelFirstJump);
            }

            return Task.CompletedTask;
        }


        private Task SetLabel(WHJumpLog? jumpLog)
        {
            this.Labels.Clear();
            this._labelFirstJump=null;
            if(_labelSize != null)
            {
                this.Labels.Add(_labelSize);
            }
            if(jumpLog != null)
            {
                _labelFirstJump=new LinkLabelModel(this, string.Format("FirstJump : {0}",jumpLog.JumpDate),null,new Point(0,-50));
                this.Labels.Add(_labelFirstJump);
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
            //SetLabel(FirstJump);
        }

        
    }
}

