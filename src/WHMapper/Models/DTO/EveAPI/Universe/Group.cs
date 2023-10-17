using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
	public class Group
	{

        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("types")]
        public int[] Types { get; set; }

        [JsonConstructor]
        public Group(int groupId, string name, bool published, int categoryId, int[] types) =>
            (GroupId, Name, Published, CategoryId, Types) = (groupId, name, published, categoryId, types);
    }
}

