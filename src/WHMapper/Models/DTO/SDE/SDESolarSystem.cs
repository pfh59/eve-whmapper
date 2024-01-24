using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace WHMapper.Models.DTO.SDE
{
	public class SDESolarSystem
	{
        /*
        [JsonIgnore]
        public bool Border { get; set; }
        [JsonIgnore]
        public IEnumerable<double> Center { get; set; }
        [JsonIgnore]
        public bool corridor { get; set; }
        
        [JsonIgnore]
        public int descriptionID {get;set;}*/
        public int factionID{get;set;}

/*
        [JsonIgnore]
        public IEnumerable<int>  DisallowedAnchorCategories { get; set; }
        [JsonIgnore]
        public IEnumerable<int> DisallowedAnchorGroups { get; set; }
        [JsonIgnore]
        public bool Fringe { get; set; }
        [JsonIgnore]
        public bool Hub { get; set; }
        [JsonIgnore]
        public bool International { get; set; }
        [JsonIgnore]
        public float Luminosity { get; set; }
        [JsonIgnore]
        public IEnumerable<double> Max { get; set; }
        [JsonIgnore]
        public IEnumerable<double> Min { get; set; }
       
        //TODO implement SDEPlanet object
        [JsonIgnore]
        public ConcurrentDictionary<int,object> Planets { get; set; }
        [JsonIgnore]
        public double Radius { get; set; }
        [JsonIgnore]
        public bool Regional { get; set; }*/

        //Only on WH
        public SDESecondarySun SecondarySun { get; set; }
        
        public float Security { get; set; }

        public string SecurityClass { get; set; }

        public int SolarSystemID { get;  set; }

        public int solarSystemNameID { get; set; }

        //TODO implement SDEStar object
        /*
        public object Star { get; set; }
        */

        public ConcurrentDictionary<int, SDEStargate> Stargates { get; set; }

        public int SunTypeID { get; set; }

        //Unknow type?
        /*
        [JsonIgnore]
        public object VisualEffect { get; set; }
        */
        //present in 0.0 solar system????
        public int WormholeClassID { get; set; }
        
        [YamlIgnore]
        public string Name { get;  set; }

        public SDESolarSystem() { }
        /*
        public SDESolarSystem(int soloarSystemId,string name)
		{
			SolarSystemID = soloarSystemId;
            Name = name;

        }*/
	}
}

