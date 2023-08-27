namespace GMB.Sdk.Core.Types.Models
{
    public class PlaceDetails
    {
        public PlaceDetails(string placeId, string name, string? firstType, string? address, string? streetNumber, string? postalCode, string? city, double? lat, double? lon, string? country, string? phone, string? phoneInternational, string? website, string? plusCode, string? url, string? status, double? rating, int? userRatingsTotal)
        {
            PlaceId = placeId;
            Name = name;
            FirstType = firstType;
            Address = address;
            StreetNumber = streetNumber;
            PostalCode = postalCode;
            City = city;
            Lat = lat;
            Long = lon;
            Country = country;
            Phone = phone;
            PhoneInternational = phoneInternational;
            Website = website;
            PlusCode = plusCode;
            Url = url;
            Status = status;
            Rating = rating;
            UserRatingsTotal = userRatingsTotal;
        }

        public string PlaceId { get; set; }
        public string Name { get; set; }
        public string? FirstType { get; set; }
        public string? Address { get; set; }
        public string? StreetNumber { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? PhoneInternational { get; set; }
        public string? Website { get; set; }
        public string? PlusCode { get; set; }
        public string? Url { get; set; }
        public string? Status { get; set; }
        public double? Rating { get; set; }
        public int? UserRatingsTotal { get; set; }
    }
}
