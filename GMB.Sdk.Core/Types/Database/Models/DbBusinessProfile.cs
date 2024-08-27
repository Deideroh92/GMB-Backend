namespace GMB.Sdk.Core.Types.Database.Models
{
    #region Enums
    public enum BusinessStatus
    {
        OPERATIONAL,
        CLOSED_TEMPORARILY,
        CLOSED_PERMANENTLY,
        DELETED
    }
    #endregion

    public class DbBusinessProfile
    {
        public long Id { get; set; }
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
        public string? LocatedIn { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public int Processing { get; set; }
        public string? PictureUrl { get; set; }
        public string? PlaceUrl { get; set; }
        public BusinessStatus Status { get; set; }

        #region Local
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
        /// <param name="locatedIn"></param>
        public DbBusinessProfile(string? placeId, string idEtab, string firstGuid, string? name, string? category, string? googleAddress, string? address, string? postCode, string? city, string? cityCode, double? lat, double? lon, string? idBan, string? addressType, string? streetNumber, double? addressScore, string? tel, string? website, string? plusCode, DateTime? dateUpdate, BusinessStatus status, string? pictureUrl, string? country, string? placeUrl, string? geoloc = null, int processing = 0, DateTime? dateInsert = null, string? telInt = null, string? locatedIn = null)
        {
            Id = -500;
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
            AddressType = addressType;
            Tel = tel;
            Website = website;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
            Status = status;
            PictureUrl = pictureUrl;
            Processing = processing;
            StreetNumber = streetNumber;
            AddressScore = addressScore;
            PlusCode = plusCode;
            Country = country;
            PlaceUrl = placeUrl;
            TelInt = telInt;
            LocatedIn = locatedIn;

            CheckValidity(); 
        }

        public DbBusinessProfile(Business? business) {
            IdEtab = business.IdEtab;
            PlaceId = business.PlaceId;
            FirstGuid = business.FirstGuid;
            Name = business.Name;
            Category = business.Category;
            Geoloc = business.Geoloc;
            GoogleAddress = business.GoogleAddress;
            Address = business.Address;
            PostCode = business.PostCode;
            City = business.City;
            CityCode = business.CityCode;
            Lat = business.Lat;
            Lon = business.Lon;
            IdBan = business.IdBan;
            AddressType = business.AddressType;
            Tel = business.Tel;
            Website = business.Website;
            DateInsert = business.DateInsert;
            DateUpdate = business.DateUpdate;
            Status = business.Status;
            PictureUrl = business.PictureUrl;
            Processing = business.Processing;
            StreetNumber = business.StreetNumber;
            AddressScore = business.AddressScore;
            PlusCode = business.PlusCode;
            Country = business.Country;
            PlaceUrl = business.PlaceUrl;
            TelInt = business.TelInt;

            CheckValidity();
        }

        public void CheckValidity()
        {
            if (IdEtab == null)
                throw new NullReferenceException("No id etab for this business");
            if (Name == null)
                Status = BusinessStatus.DELETED;
        }

        public bool Equals(DbBusinessProfile? other)
        {
            return other is not null &&
                   PlaceId == other.PlaceId &&
                   Name == other.Name &&
                   Category == other.Category &&
                   GoogleAddress == other.GoogleAddress &&
                   Tel == other.Tel &&
                   Website == other.Website &&
                   Status == other.Status &&
                   Country == other.Country &&
                   PictureUrl == other.PictureUrl &&
                   LocatedIn == other.LocatedIn;
        }

        public bool AdressEquals(DbBusinessProfile? other)
        {
            return other is not null && GoogleAddress == other.GoogleAddress && LocatedIn == other.LocatedIn;
        }
        #endregion
    }
}