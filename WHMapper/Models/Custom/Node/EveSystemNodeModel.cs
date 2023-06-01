using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using WHMapper.Models.Db;

namespace WHMapper.Models.Custom.Node
{
    public class EveSystemNodeModel : NodeModel
    {
        public event Action<EveSystemNodeModel>? OnLocked;

        private WHSystem _wh = null!;

        public int IdWH
        {
            get
            {
                return _wh.Id;

            }
        }

        public int IdWHMap
        {
            get
            {
                return _wh.WHMapId;

            }
        }

        public String Name
        {
            get
            {
                return _wh.Name;
            }
         }

        public String? NameExtension
        {
            get
            {
                if(_wh!=null && _wh.NameExtension!=0)
                    return Convert.ToChar(_wh.NameExtension).ToString();
                return null;
            }
        }

        
        public int SolarSystemId
        {
            get
            {
                return _wh.SoloarSystemId;
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

        
        public new bool Locked
        {
            get
            {
                return base.Locked;
            }
            set
            {
                if (base.Locked != value)
                {
                    base.Locked = value;
                    OnLocked?.Invoke(this);
                }
            }

        }

        
        public String Class { get; private set; } = null!;
        public String Effect { get; private set; } = null!;
        public IEnumerable<KeyValuePair<string, string>> Statics { get; private set; } = null!;
        public IEnumerable<KeyValuePair<string, string>> EffectsInfos { get; private set; } = null!;
        public BlockingCollection<string> ConnectedUsers { get; private set; } = new BlockingCollection<string>();


        public EveSystemNodeModel(WHSystem wh, string whClass, string whEffects, IEnumerable<KeyValuePair<string, string>> whEffectsInfos, IEnumerable<KeyValuePair<string, string>> whStatics) 
        {
            _wh = wh;

            Title = this.Name;
            Class = whClass.ToUpper();
            Effect = whEffects;
            EffectsInfos = whEffectsInfos;
            Statics = whStatics;
            Locked = wh.Locked;

            AddPort(PortAlignment.Bottom);
            AddPort(PortAlignment.Top);
            AddPort(PortAlignment.Left);
            AddPort(PortAlignment.Right);


        }

        public EveSystemNodeModel(WHSystem wh)
        {
            _wh = wh;
            Title = this.Name;
            Locked = wh.Locked;

            if (SecurityStatus >= 0.5)
                Class = "H";
            else if (SecurityStatus < 0.5 && SecurityStatus > 0)
                Class = "LS";
            else
                Class = "0.0";

            AddPort(PortAlignment.Bottom);
            AddPort(PortAlignment.Top);
            AddPort(PortAlignment.Left);
            AddPort(PortAlignment.Right);
        }


        public void IncrementNameExtension()
        {
            if (_wh.NameExtension == 0)
                _wh.NameExtension = Convert.ToByte('A');
            else
            {
                if (_wh.NameExtension != Convert.ToByte('Z'))
                    _wh.NameExtension++;
                else
                    _wh.NameExtension = Convert.ToByte('Z');
            }
        }

        public void DecrementNameExtension()
        {
            if (_wh.NameExtension > Convert.ToByte('A'))
                _wh.NameExtension--;
            else
                _wh.NameExtension = 0;
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

