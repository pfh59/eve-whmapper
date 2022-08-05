using System;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace WHMapper.Models.Custom.Node
{
    public class EveSystemNodeModel : NodeModel
    {
        public String Name { get; private set; }
        public float SecurityStatus { get; private set; }
        public String Class { get; private set; }
        public String Effect { get; private set; }
        public IEnumerable<String> Statics { get; private set; }

        public EveSystemNodeModel(string name, float securityStatus, string whClass, string whEffects, IEnumerable<string> whStatics) : base(shape: Shapes.Rectangle)
        {
            Name = name;
            SecurityStatus = securityStatus;
            Class = whClass.ToUpper();
            Effect = whEffects;
            Statics = whStatics;

            Title = name;
        }

        public EveSystemNodeModel(string name, float securityStatus)
        {
            Name = name;
            SecurityStatus = securityStatus;
            if (SecurityStatus >= 0.5)
            {
                Class = "H";
            }
            else if (SecurityStatus < 0.5 && SecurityStatus > 0)
            {
                Class = "LS";
            }
            else
            {
                Class = "0.0";
            }
            Title = name;
        }
    }
}

