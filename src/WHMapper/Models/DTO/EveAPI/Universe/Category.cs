using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Category
    {
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("groups")]
        public int[] Groups { get; set; }


        [JsonConstructor]
        public Category(int categoryId, int[] groups,string name, bool published) =>
        (CategoryId, Groups,Name, Published) = (categoryId, groups,name, published);

    }

}

