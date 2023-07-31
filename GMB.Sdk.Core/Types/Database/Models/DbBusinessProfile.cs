namespace GMB.Sdk.Core.Types.Database.Models
{
    #region Enums

    public enum BusinessStatus
    {
        OPEN,
        TEMPORARLY_CLOSED,
        CLOSED,
        DELETED
    }

    #endregion

    public class DbBusinessProfile
    {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public string FirstGuid { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Geoloc { get; set; }
        public string? GoogleAddress { get; set; }
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? City { get; set; }
        public string? CityCode { get; set; }
        public float? Lat { get; set; }
        public float? Lon { get; set; }
        public string? IdBan { get; set; }
        public string? StreetNumber { get; set; }
        public string? Country { get; set; }
        public string? AddressType { get; set; }
        public float? AddressScore { get; set; }
        public string? Tel { get; set; }
        public string? Website { get; set; }
        public string? PlusCode { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public BusinessStatus Status { get; set; }
        public int Processing { get; set; }
        public string? PictureUrl { get; set; }

        #region Local
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idEtab"></param>
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
        public DbBusinessProfile(string idEtab, string firstGuid, string? name, string? category, string? googleAddress, string? address, string? postCode, string? city, string? cityCode, float? lat, float? lon, string? idBan, string? addressType, string? streetNumber, float? addressScore, string? tel, string? website, string? plusCode, DateTime? dateUpdate, BusinessStatus status, string? pictureUrl, string? country, string? geoloc = null, int processing = 0, DateTime? dateInsert = null)
        {
            IdEtab = idEtab;
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
            Country = Country;

            CheckValidity();
        }

        public void CheckValidity()
        {
            if (IdEtab == null)
                throw new NullReferenceException("No IdEtab for this business");
            if (Name == null)
                Status = BusinessStatus.DELETED;
        }
        #endregion
    }
}