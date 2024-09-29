namespace GMB.Sdk.Core.Types.Database.Models
{

    public class DbPlace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? City { get; set; }
        public double? Score { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Url { get; set; }
        public string? Country { get; set; }
        public string? Tel { get; set; }
        public string? TelInt { get; set; }
        public int? NbReviews { get; set; }
        public string? Website { get; set; }
        public string? PlusCode { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public BusinessStatus Status { get; set; }

        #region Local
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="address"></param>
        /// <param name="postCode"></param>
        /// <param name="city"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="tel"></param>
        /// <param name="website"></param>
        /// <param name="dateInsert"></param>
        /// <param name="dateUpdate"></param>
        /// <param name="status"></param>
        /// <param name="plusCode"></param>
        /// <param name="country"></param>
        /// <param name="url"></param>
        /// <param name="telInt"></param>
        /// <param name="score"></param>
        /// <param name="nbReviews"></param>
        public DbPlace(string id, string name, string? category, string? address, string? postCode, string? city, double? lat, double? lon, string? tel, string? telInt, string? website, string? plusCode, BusinessStatus status, string? country, string url, double? score, int? nbReviews, DateTime? dateInsert = null, DateTime? dateUpdate = null)
        {
            Id = id;
            Name = name;
            Category = category;
            Address = address;
            PostCode = postCode;
            City = city;
            Lat = lat;
            Lon = lon;
            Tel = tel;
            TelInt = telInt;
            Website = website;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
            Status = status;
            Url = url;
            PlusCode = plusCode;
            Country = country;
            Score = score;
            NbReviews = nbReviews;

            CheckValidity();
        }

        public void CheckValidity()
        {
            if (Id == null)
                throw new NullReferenceException("No place id for this place");
            if (Name == null)
                Status = BusinessStatus.DELETED;
        }

        public bool Equals(DbPlace? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   Name == other.Name &&
                   Category == other.Category &&
                   Address == other.Address &&
                   Tel == other.Tel &&
                   Website == other.Website &&
                   Status == other.Status &&
                   Country == other.Country &&
                   Url == other.Url &&
                   NbReviews == other.NbReviews &&
                   Score == other.Score;
        }
        #endregion
    }
}