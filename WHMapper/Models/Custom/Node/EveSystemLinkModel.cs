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
                //SetLabel(_whLink.IsEndOfLifeConnection,_whLink.Size);
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
                SetLabel(_whLink.IsEndOfLifeConnection,_whLink.Size);
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
                //Color = GetLinkStatusColor(_whLink.MassStatus);
                //SelectedColor = GetLinkStatusColor(_whLink.MassStatus);
            }
        }

        private void SetLabel(bool isEol,SystemLinkSize size)
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
                //case SystemLinkSize.Large:
                //    this.Labels.Add(new LinkLabelModel(this, "L"));
                //    break;
                case SystemLinkSize.XLarge:
                    this.Labels.Add(new LinkLabelModel(this, "XL"));
                    break;
            }
            /*
            if(isEol)
            {
                this.Labels.Add(new LinkLabelModel(this, "EOL"));
            }*/
        }

        /*
        private string GetLinkStatusColor(SystemLinkMassStatus status)
        {
            switch (status)
            {
                case SystemLinkMassStatus.Normal:
                    return "#3C3F41";
                case SystemLinkMassStatus.Critical:
                    return "#e28a0d";
                case SystemLinkMassStatus.Verge:
                    return "#a52521";
            }

            return "#3C3F41";
        }*/


        public EveSystemLinkModel(WHSystemLink whLink,EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort)
            : base (sourcePort, targetPort)
        {
            _whLink = whLink;
            //Color = GetLinkStatusColor(_whLink.MassStatus);
            //SelectedColor = GetLinkStatusColor(_whLink.MassStatus);
            SetLabel(_whLink.IsEndOfLifeConnection,_whLink.Size);
            
        }

    }
}

