using Newtonsoft.Json;

namespace GMB.Sdk.Core.Types.Models
{
    public class PlaceIdResponse
    {
        [JsonProperty("candidates")]
        public List<Candidate>? PlaceIds { get; set; }
        [JsonProperty("status")]
        public string? ResponseStatus { get; set; }

        public static PlaceIdResponse? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PlaceIdResponse>(json);
        }
    }

    public class Candidate
    {
        [JsonProperty("place_id")]
        public string? PlaceId { get; set; }
    }
}