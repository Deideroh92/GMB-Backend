using Newtonsoft.Json;

namespace GMS.Sdk.Core.Models
{
    public class AddressApiResponse
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("features")]
        public Feature[]? Features { get; set; }

        [JsonProperty("attribution")]
        public string? Attribution { get; set; }

        [JsonProperty("licence")]
        public string? Licence { get; set; }

        [JsonProperty("query")]
        public string? Query { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        public static AddressApiResponse? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AddressApiResponse>(json);
        }
    }

    public class Feature
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("geometry")]
        public Geometry? Geometry { get; set; }

        [JsonProperty("properties")]
        public Properties? Properties { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("coordinates")]
        public double[]? Coordinates { get; set; }
    }

    public class Properties
    {
        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("score")]
        public double? Score { get; set; }

        [JsonProperty("housenumber")]
        public string? HouseNumber { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("postcode")]
        public string? Postcode { get; set; }

        [JsonProperty("citycode")]
        public string? CityCode { get; set; }

        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("context")]
        public string? Context { get; set; }

        [JsonProperty("type")]
        public string? PropertyType { get; set; }

        [JsonProperty("importance")]
        public double? Importance { get; set; }

        [JsonProperty("street")]
        public string? Street { get; set; }
    }
}