namespace GMS.Sdk.Core.Database {
    #region Enums

    public enum BusinessStatus {
        OPEN,
        TEMPORARLY_CLOSED,
        CLOSED,
        DELETED
    }

    #endregion

    public class DbBusinessProfile : IEquatable<DbBusinessProfile?> {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public string FirstGuid { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Adress { get; set; }
        public string? Tel { get; set; }
        public string? Website { get; set; }
        public string? Geoloc { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public BusinessStatus Status { get; set; }
        public int Processing { get; set; }
        public string PictureUrl { get; set; }

        #region Equality

        public override bool Equals(object? obj) {
            return Equals(obj as DbBusinessProfile);
        }

        public bool Equals(DbBusinessProfile? other) {
            return other is not null &&
                   Name == other.Name &&
                   Category == other.Category &&
                   Adress == other.Adress &&
                   Tel == other.Tel &&
                   Website == other.Website;
        }

        public override int GetHashCode() {
            HashCode hash = new();
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
        /// Constructor
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
        public DbBusinessProfile(string idEtab, string firstGuid, string? name, string? category, string? adress, string? tel, string? website, string? geoloc, DateTime? dateInsert, DateTime? dateUpdate, BusinessStatus status, string pictureUrl, int processing = 0) {
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
            PictureUrl= pictureUrl;
            Processing = processing;

            if (DateInsert == null)
                DateInsert = DateTime.UtcNow;

            if (DateUpdate == null)
                DateUpdate = DateTime.UtcNow;

            CheckValidity();
        }

        public void CheckValidity() {
            if (IdEtab == null)
                throw new NullReferenceException("No IdEtab for this business");
            if (Name == null)
                Status = BusinessStatus.DELETED;
        }
        #endregion
    }
}