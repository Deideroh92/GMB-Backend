namespace GMS.Sdk.Core.Database
{
    #region Enums

    public enum BusinessStatus
    {
        OPEN,
        TEMPORARLY_CLOSED,
        CLOSED
    }

    #endregion

    public class DbBusinessProfile : IEquatable<DbBusinessProfile?>
    {
        public long Id { get; set; }
        public long IdEtab { get; set; }
        public Guid FirstGuid { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? Adress { get; set; }
        public string? Tel { get; set; }
        public string? Website { get; set; }
        public string? Geoloc { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public BusinessStatus Status { get; set; }
        public bool Processing { get; set; }

        #region Equality

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DbBusinessProfile);
        }

        public bool Equals(DbBusinessProfile? obj)
        {
            return obj is DbBusinessProfile profile &&
                   IdEtab == profile.IdEtab &&
                   FirstGuid.Equals(profile.FirstGuid) &&
                   Name == profile.Name &&
                   Category == profile.Category &&
                   Adress == profile.Adress &&
                   Tel == profile.Tel &&
                   Website == profile.Website &&
                   Geoloc == profile.Geoloc &&
                   DateInsert == profile.DateInsert &&
                   DateUpdate == profile.DateUpdate &&
                   Status == profile.Status &&
                   Processing == profile.Processing;
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(IdEtab);
            hash.Add(FirstGuid);
            hash.Add(Name);
            hash.Add(Category);
            hash.Add(Adress);
            hash.Add(Tel);
            hash.Add(Website);
            hash.Add(Geoloc);
            hash.Add(DateInsert);
            hash.Add(DateUpdate);
            hash.Add(Status);
            hash.Add(Processing);
            return hash.ToHashCode();
        }
        #endregion

        #region Local

        /// <summary>
        /// Create a new instance of Business Profile.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="firstGuid"></param>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="adress"></param>
        /// <param name="tel"></param>
        /// <param name="website"></param>
        /// <param name="geoloc"></param>
        /// <param name="dateInsert"></param>
        /// <param name="dateUpdate"></param>
        /// <param name="status"></param>
        /// <param name="processing"></param>
        public DbBusinessProfile(long idEtab, Guid firstGuid, string name, string? category, string? adress, string? tel, string? website, string? geoloc, DateTime? dateInsert, DateTime? dateUpdate, BusinessStatus status, bool processing = false)
        {
            IdEtab = idEtab;
            FirstGuid = firstGuid;
            Name = name;
            Category = category;
            Adress = adress;
            Tel = tel;
            Website = website;
            Geoloc = geoloc;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
            Status = status;
            Processing = processing;
        }

        /// <summary>
        /// Save object in DB.
        /// </summary>
        public void Save()
        {

        }
        #endregion
    }
}