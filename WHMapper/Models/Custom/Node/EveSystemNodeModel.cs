using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using WHMapper.Models.Db;

namespace WHMapper.Models.Custom.Node
{
    public class EveSystemNodeModel : NodeModel
    {
        private WHSystem _wh;


        public int IdWH
        {
            get
            {
                return _wh.Id;

            }
        }

        public String Name
        {
            get
            {
                return _wh.Name;
            }
         }

        public string? NameExtension
        {
            get
            {
                if(_wh.NameExtension!=null && _wh.NameExtension!=0)
                    return Convert.ToChar(_wh.NameExtension).ToString();
                return null;
            }
        }

        public float SecurityStatus
        {
            get
            {
                if (_wh.SecurityStatus <= -0.99)
                    return -1;
                else
                    return (float)Math.Round(_wh.SecurityStatus, 1);

            }
        }


        public String? Class { get; private set; }
        public String? Effect { get; private set; }
        public IEnumerable<KeyValuePair<string, string>>? Statics { get; private set; }
        public IEnumerable<KeyValuePair<string, string>>? EffectsInfos { get; private set; }
        public BlockingCollection<string> ConnectedUsers { get; private set; } = new BlockingCollection<string>();

        public EveSystemNodeModel(WHSystem wh, string whClass, string whEffects, IEnumerable<KeyValuePair<string, string>> whEffectsInfos, IEnumerable<KeyValuePair<string, string>> whStatics) 
        {
            _wh = wh;

            Title = this.Name;
            Class = whClass.ToUpper();
            Effect = whEffects;
            EffectsInfos = whEffectsInfos;
            Statics = whStatics;
        }

        public EveSystemNodeModel(WHSystem wh)
        {
            _wh = wh;
            Title = this.Name;

            if (SecurityStatus >= 0.5)
                Class = "H";
            else if (SecurityStatus < 0.5 && SecurityStatus > 0)
                Class = "LS";
            else
                Class = "0.0";
        
        }


        public async Task AddConnectedUser(string userName)
        {
            if (!ConnectedUsers.Contains(userName))
                while (!ConnectedUsers.TryAdd(userName))
                    await Task.Delay(1);
        }

        public async Task RemoveConnectedUser(string userName)
        {
            if (ConnectedUsers.Contains(userName))
            {

                string comparedItem;
                var itemsList = new List<string>();
                do
                {

                    while (!ConnectedUsers.TryTake(out comparedItem))
                        await Task.Delay(1);

                    if (!comparedItem.Equals(userName))
                    {
                        itemsList.Add(comparedItem);
                    }
                } while (!(comparedItem.Equals(userName)));
                Parallel.ForEach(itemsList, async t => await AddConnectedUser(t));
            }
        }
    }
}

