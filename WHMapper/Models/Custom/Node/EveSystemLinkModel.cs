using System;
using System.ComponentModel.DataAnnotations;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Extensions;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;

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
                this.Labels.Clear();
                switch (_whLink.Size)
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


    public EveSystemLinkModel(WHSystemLink whLink,EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort)
            : base (sourcePort, targetPort)
        {
            _whLink = whLink;
        }

    }
}

