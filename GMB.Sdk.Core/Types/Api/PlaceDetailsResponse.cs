using Newtonsoft.Json;

namespace GMB.Sdk.Core.Types.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AddressComponent
    {
        [JsonProperty("long_name")]
        public string? LongName { get; set; }

        [JsonProperty("short_name")]
        public string? ShortName { get; set; }

        [JsonProperty("types")]
        public List<string>? Types { get; set; }
    }

    public class GeometryLocation
    {
        [JsonProperty("lat")]
        public double? Latitude { get; set; }

        [JsonProperty("lng")]
        public double? Longitude { get; set; }
    }

    public class GeometryViewport
    {
        [JsonProperty("northeast")]
        public GeometryLocation? Northeast { get; set; }

        [JsonProperty("southwest")]
        public GeometryLocation? Southwest { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("location")]
        public GeometryLocation? Location { get; set; }

        [JsonProperty("viewport")]
        public GeometryViewport? Viewport { get; set; }
    }

    public class PlusCode
    {
        [JsonProperty("compound_code")]
        public string? CompoundCode { get; set; }

        [JsonProperty("global_code")]
        public string? GlobalCode { get; set; }
    }

    public class Result
    {
        [JsonProperty("address_components")]
        public List<AddressComponent>? AddressComponents { get; set; }
        [JsonProperty("formatted_address")]
        public string? FormattedAdress { get; set; }

        [JsonProperty("business_status")]
        public string? BusinessStatus { get; set; }

        [JsonProperty("formatted_phone_number")]
        public string? FormattedPhoneNumber { get; set; }

        [JsonProperty("geometry")]
        public Geometry? Geometry { get; set; }

        [JsonProperty("international_phone_number")]
        public string? InternationalPhoneNumber { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("place_id")]
        public string? PlaceId { get; set; }

        [JsonProperty("plus_code")]
        public PlusCode? PlusCode { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("user_ratings_total")]
        public int? UserRatingsTotal { get; set; }

        [JsonProperty("website")]
        public string? Website { get; set; }
        [JsonProperty("types")]
        public List<string>? Types { get; set; }
    }

    public class PlaceDetailsResponse
    {
        [JsonProperty("html_attributions")]
        public List<object>? HtmlAttributions { get; set; }

        [JsonProperty("result")]
        public Result? Result { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        public static PlaceDetailsResponse? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PlaceDetailsResponse>(json);
        }
    }

}