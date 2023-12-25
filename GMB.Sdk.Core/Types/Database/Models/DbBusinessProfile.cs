using System.Runtime.Serialization;

namespace GMB.Sdk.Core.Types.Database.Models
{
    #region Enums

    public enum BusinessStatus
    {
        [EnumMember(Value = "OPERATIONAL")]
        OPERATIONAL,

        [EnumMember(Value = "CLOSED_TEMPORARILY")]
        CLOSED_TEMPORARILY,

        [EnumMember(Value = "CLOSED_PERMANENTLY")]
        CLOSED_PERMANENTLY,

        [EnumMember(Value = "DELETED")]
        DELETED
    }

    #endregion


    public class DbBusinessProfile : IEquatable<DbBusinessProfile?>
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
        public DbBusinessProfile(string? placeId, string idEtab, string firstGuid, string? name, string? category, string? googleAddress, string? address, string? postCode, string? city, string? cityCode, double? lat, double? lon, string? idBan, string? addressType, string? streetNumber, double? addressScore, string? tel, string? website, string? plusCode, DateTime? dateUpdate, BusinessStatus status, string? pictureUrl, string? country, string? urlPlace, string? geoloc = null, int processing = 0, DateTime? dateInsert = null, string? telInt = null)
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
            PlaceUrl = urlPlace;
            TelInt = telInt;

            CheckValidity();
            TelInt = telInt;
        }

        public void CheckValidity()
        {
            if (IdEtab == null)
                throw new NullReferenceException("No IdEtab for this business");
            if (Name == null)
                Status = BusinessStatus.DELETED;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DbBusinessProfile);
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
                   PictureUrl == other.PictureUrl;
        }

        public bool AdressEquals(DbBusinessProfile? other)
        {
            return other is not null && GoogleAddress == other.GoogleAddress;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Id);
            hash.Add(IdEtab);
            hash.Add(PlaceId);
            hash.Add(FirstGuid);
            hash.Add(Name);
            hash.Add(Category);
            hash.Add(Geoloc);
            hash.Add(GoogleAddress);
            hash.Add(Address);
            hash.Add(PostCode);
            hash.Add(City);
            hash.Add(CityCode);
            hash.Add(Lat);
            hash.Add(Lon);
            hash.Add(IdBan);
            hash.Add(StreetNumber);
            hash.Add(Country);
            hash.Add(AddressType);
            hash.Add(AddressScore);
            hash.Add(Tel);
            hash.Add(Website);
            hash.Add(PlusCode);
            hash.Add(DateInsert);
            hash.Add(DateUpdate);
            hash.Add(Status);
            hash.Add(Processing);
            hash.Add(PictureUrl);
            return hash.ToHashCode();
        }
        #endregion
    }
}