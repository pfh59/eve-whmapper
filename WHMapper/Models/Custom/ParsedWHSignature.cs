using System;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Models.Custom
{
	public class ParsedWHSignature
	{
        public string Name { get; set; }
        public WHSignatureGroup Group { get; set; }
        public string Type { get; set; }

        public ParsedWHSignature(string name, WHSignatureGroup group,string type)
		{
            Name = name;
            Group = group;
            Type = type;
        }
	}
}

