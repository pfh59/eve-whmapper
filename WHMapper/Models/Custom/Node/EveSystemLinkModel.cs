using System;
using System.ComponentModel.DataAnnotations;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Extensions;
using WHMapper.Models.Db;

namespace WHMapper.Models.Custom.Node
{

    public class EveSystemLinkModel : LinkModel
    {
        public string? EolColor
        {
            get
            {
                return "#d747d6";
            }
             
        }

        
        public bool IsEoL { get; set; }


        private SystemLinkSize _size;
        public SystemLinkSize Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                this.Labels.Clear();
                switch (_size)
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

        private SystemLinkMassStatus _massStatus;
        public SystemLinkMassStatus MassStatus
        {
            get
            {
                return _massStatus;
            }
            set
            {
                _massStatus = value;
                switch (_massStatus)
                {
                    case SystemLinkMassStatus.Normal:
                        this.Color = "#3C3F41";
                        break;
                    case SystemLinkMassStatus.Critical:
                        this.Color = "#e28a0d";

                        break;
                    case SystemLinkMassStatus.Verge:
                        this.Color = "#a52521";                                               
                        break;

                }
            }
        } 

        public EveSystemLinkModel(EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort)
            : this (sourcePort, targetPort, false,SystemLinkSize.Large,SystemLinkMassStatus.Normal) { }

        public EveSystemLinkModel(EveSystemNodeModel sourcePort, EveSystemNodeModel targetPort,bool isEol, SystemLinkSize size, SystemLinkMassStatus mass):
            base(sourcePort, targetPort)
        {
            SelectedColor = "white";
            IsEoL = isEol;
            Size = size;
            MassStatus = mass;
            
        }


    }
}

