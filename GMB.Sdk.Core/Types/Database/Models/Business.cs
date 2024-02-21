namespace GMB.Sdk.Core.Types.Database.Models
{
    public class Business
    {
        public string? Id { get; set; }
        public string IdEtab { get; set; }
        public string? PlaceId { get; set; }
        public string FirstGuid { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Geoloc { get; set; }
        public string? GoogleAddress { get; set; }
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? City { get; set; }
        public string? CityCode { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string? IdBan { get; set; }
        public string? StreetNumber { get; set; }
        public string? Country { get; set; }
        public string? AddressType { get; set; }
        public double? AddressScore { get; set; }
        public string? Tel { get; set; }
        public string? TelInt { get; set; }
        public string? Website { get; set; }
        public string? PlusCode { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string? PictureUrl { get; set; }
        public string? PlaceUrl { get; set; }
        public BusinessStatus Status { get; set; }
        public int Processing { get; set; }
        public double? Score { get; set; }
        public int? NbReviews { get; set; }
        public List<DbBusinessReview>? Reviews { get; set; }

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
        /// <param name="processing"></param>
        /// <param name="score"></param>
        /// <param name="nbReviews"></param>
        /// <param name="reviews"></param>
        public Business(string? placeId, string idEtab, string firstGuid, string? name, string? category, string? googleAddress, string? address, string? postCode, string? city, string? cityCode, double? lat, double? lon, string? idBan, string? addressType, string? streetNumber, double? addressScore, string? tel, string? website, string? plusCode, DateTime? dateUpdate, BusinessStatus status, string? pictureUrl, string? country, string? placeUrl, int processing, string? geoloc = null, DateTime? dateInsert = null, string? telInt = null, double? score = null, int? nbReviews = null, List<DbBusinessReview>? reviews = null)
        {
            IdEtab = idEtab;
            PlaceId = placeId;
            FirstGuid = firstGuid;
            Name = name;
            Category = category;
            Geoloc = geoloc;
            GoogleAddress = googleAddress;
            Address = address;
            PostCode = postCode;
            City = city;
            CityCode = cityCode;
            Lat = lat;
            Lon = lon;
            IdBan = idBan;
            StreetNumber = streetNumber;
            Country = country;
            AddressType = addressType;
            AddressScore = addressScore;
            Tel = tel;
            TelInt = telInt;
            Website = website;
            PlusCode = plusCode;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
            PictureUrl = pictureUrl;
            PlaceUrl = placeUrl;
            Status = status;
            Processing = processing;
            Score = score;
            NbReviews = nbReviews;
            Reviews = reviews;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bp"></param>
        /// <param name="bs"></param>
        /// <param name="reviews"></param>
        public Business(DbBusinessProfile bp, DbBusinessScore? bs = null, List<DbBusinessReview>? reviews = null)
        {
            IdEtab = bp.IdEtab;
            PlaceId = bp.PlaceId;
            FirstGuid = bp.FirstGuid;
            Name = bp.Name;
            Category = bp.Category;
            Geoloc = bp.Geoloc;
            GoogleAddress = bp.GoogleAddress;
            Address = bp.Address;
            PostCode = bp.PostCode;
            City = bp.City;
            CityCode = bp.CityCode;
            Lat = bp.Lat;
            Lon = bp.Lon;
            IdBan = bp.IdBan;
            StreetNumber = bp.StreetNumber;
            Country = bp.Country;
            AddressType = bp.AddressType;
            AddressScore = bp.AddressScore;
            Tel = bp.Tel;
            TelInt = bp.TelInt;
            Website = bp.Website;
            PlusCode = bp.PlusCode;
            DateInsert = bp.DateInsert;
            DateUpdate = bp.DateUpdate;
            PictureUrl = bp.PictureUrl;
            PlaceUrl = bp.PlaceUrl;
            Status = bp.Status;
            Processing = bp.Processing;
            Score = bs?.Score;
            NbReviews = bs?.NbReviews;
            Reviews = reviews;
        }
    }
}