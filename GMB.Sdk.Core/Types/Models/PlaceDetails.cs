namespace GMB.Sdk.Core.Types.Models
{
    public class PlaceDetails(string placeId, string name, string? firstType, string? address, string? streetNumber, string? postalCode, string? city, double? lat, double? lon, string? country, string? phone, string? phoneInternational, string? website, string? plusCode, string? url, string? status, double? rating, int? userRatingsTotal)
    {
        public string PlaceId { get; set; } = placeId;
        public string Name { get; set; } = name;
        public string? FirstType { get; set; } = firstType;
        public string? Address { get; set; } = address;
        public string? StreetNumber { get; set; } = streetNumber;
        public string? PostalCode { get; set; } = postalCode;
        public string? City { get; set; } = city;
        public double? Lat { get; set; } = lat;
        public double? Long { get; set; } = lon;
        public string? Country { get; set; } = country;
        public string? Phone { get; set; } = phone;
        public string? PhoneInternational { get; set; } = phoneInternational;
        public string? Website { get; set; } = website;
        public string? PlusCode { get; set; } = plusCode;
        public string? Url { get; set; } = url;
        public string? Status { get; set; } = status;
        public double? Rating { get; set; } = rating;
        public int? UserRatingsTotal { get; set; } = userRatingsTotal;
    }
}
