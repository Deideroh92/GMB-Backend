namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="idEtab"></param>
    /// <param name="placeId"></param>
    /// <param name="firstGuid"></param>
    /// <param name="name"></param>
    /// <param name="category"></param>
    /// <param name="googleAddress"></param>
    /// <param name="address"></param>
    /// <param name="postCode"></param>
    /// <param name="city"></param>
    /// <param name="cityCode"></param>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <param name="idBan"></param>
    /// <param name="addressType"></param>
    /// <param name="tel"></param>
    /// <param name="website"></param>
    /// <param name="dateInsert"></param>
    /// <param name="dateUpdate"></param>
    /// <param name="status"></param>
    /// <param name="addressScore"></param>
    /// <param name="plusCode"></param>
    /// <param name="country"></param>
    public class BusinessProfileSmooth(string? placeId, string idEtab, string firstGuid, string? name, string? category, string? googleAddress, string? address, string? postCode, string? city, string? cityCode, double? lat, double? lon, string? idBan, string? addressType, string? streetNumber, double? addressScore, string? tel, string? website, string? plusCode, DateTime? dateUpdate, BusinessStatus status, string? pictureUrl, string? country, string? urlPlace, string? geoloc = null, DateTime? dateInsert = null, string? telInt = null)
    {
        public long Id { get; set; } = -500;
        public string IdEtab { get; set; } = idEtab;
        public string? PlaceId { get; set; } = placeId;
        public string FirstGuid { get; set; } = firstGuid;
        public string? Name { get; set; } = name;
        public string? Category { get; set; } = category;
        public string? Geoloc { get; set; } = geoloc;
        public string? GoogleAddress { get; set; } = googleAddress;
        public string? Address { get; set; } = address;
        public string? PostCode { get; set; } = postCode;
        public string? City { get; set; } = city;
        public string? CityCode { get; set; } = cityCode;
        public double? Lat { get; set; } = lat;
        public double? Lon { get; set; } = lon;
        public string? IdBan { get; set; } = idBan;
        public string? StreetNumber { get; set; } = streetNumber;
        public string? Country { get; set; } = country;
        public string? AddressType { get; set; } = addressType;
        public double? AddressScore { get; set; } = addressScore;
        public string? Tel { get; set; } = tel;
        public string? TelInt { get; set; } = telInt;
        public string? Website { get; set; } = website;
        public string? PlusCode { get; set; } = plusCode;
        public DateTime? DateInsert { get; set; } = dateInsert;
        public DateTime? DateUpdate { get; set; } = dateUpdate;
        public string? PictureUrl { get; set; } = pictureUrl;
        public string? PlaceUrl { get; set; } = urlPlace;
        public BusinessStatus Status { get; set; } = status;
    }
}