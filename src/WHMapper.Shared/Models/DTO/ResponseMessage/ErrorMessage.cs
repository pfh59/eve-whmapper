using System.Text.Json.Serialization;

namespace WHMapper.Shared.Models.DTO.ResponseMessage
{
    public class ErrorMessage : IResponseMessage
    {
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

    }
}
