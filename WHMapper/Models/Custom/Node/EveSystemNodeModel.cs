using System;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace WHMapper.Models.Custom.Node
{
    public class EveSystemNodeModel : NodeModel
    {
        public String Name { get; private set; }
        public float SecurityStatus { get; private set; }
        public String? Class { get; private set; }
        public String? Effect { get; private set; }
        public IEnumerable<KeyValuePair<string, string>>? Statics { get; private set; }
        public IEnumerable<KeyValuePair<string, string>>? EffectsInfos { get; private set; }

        

    public EveSystemNodeModel(string name, float securityStatus, string whClass, string whEffects, IEnumerable<KeyValuePair<string,string>> whEffectsInfos, IEnumerable<KeyValuePair<string, string>> whStatics) : base(shape: Shapes.Rectangle)
        {
            Name = name;

            if (securityStatus <= -0.99)
                SecurityStatus = -1;
            else
                SecurityStatus = (float)Math.Round(securityStatus, 1);
       
            Class = whClass.ToUpper();
            Effect = whEffects;
            EffectsInfos = whEffectsInfos;
            Statics = whStatics;

            Title = name;
        }

        public EveSystemNodeModel(string name, float securityStatus)
        {
            Name = name;
            if(securityStatus<= -0.99)
                SecurityStatus = -1;
            else
                SecurityStatus = (float)Math.Round(securityStatus, 1);

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

