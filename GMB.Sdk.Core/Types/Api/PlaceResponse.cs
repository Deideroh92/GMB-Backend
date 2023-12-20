using Newtonsoft.Json;

public class AddressComponent
{
    [JsonProperty("longText")]
    public string? LongText { get; set; }

    [JsonProperty("shortText")]
    public string? ShortText { get; set; }

    [JsonProperty("types")]
    public string[]? Types { get; set; }

    [JsonProperty("languageCode")]
    public string? LanguageCode { get; set; }
}

public class PlusCode
{
    [JsonProperty("globalCode")]
    public string? GlobalCode { get; set; }

    [JsonProperty("compoundCode")]
    public string? CompoundCode { get; set; }
}

public class Location
{
    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }
}

public class DisplayName
{
    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("languageCode")]
    public string? LanguageCode { get; set; }
}

public class AuthorAttribution
{
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }

    [JsonProperty("uri")]
    public string? Uri { get; set; }

    [JsonProperty("photoUri")]
    public string? PhotoUri { get; set; }
}

public class Place
{
    [JsonProperty("id")]
    public required string PlaceId { get; set; }
    [JsonProperty("nationalPhoneNumber")]
    public string? NationalPhoneNumber { get; set; }

    [JsonProperty("internationalPhoneNumber")]
    public string? InternationalPhoneNumber { get; set; }

    [JsonProperty("formattedAddress")]
    public string? FormattedAddress { get; set; }

    [JsonProperty("addressComponents")]
    public AddressComponent[]? AddressComponents { get; set; }

    [JsonProperty("plusCode")]
    public PlusCode? PlusCode { get; set; }

    [JsonProperty("location")]
    public Location? Location { get; set; }

    [JsonProperty("rating")]
    public double Rating { get; set; }

    [JsonProperty("googleMapsUri")]
    public string? GoogleMapsUri { get; set; }

    [JsonProperty("websiteUri")]
    public string? WebsiteUri { get; set; }

    [JsonProperty("businessStatus")]
    public string? BusinessStatus { get; set; }

    [JsonProperty("userRatingCount")]
    public int? UserRatingCount { get; set; }

    [JsonProperty("displayName")]
    public DisplayName? DisplayName { get; set; }

    [JsonProperty("shortFormattedAddress")]
    public string? ShortFormattedAddress { get; set; }

    public static Place? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Place>(json);
    }
}

public class Root
{
    [JsonProperty("places")]
    public Place[]? Places { get; set; }
    public static Root? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Root>(json);
    }
}
