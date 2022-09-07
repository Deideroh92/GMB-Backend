namespace GMS.Sdk.Core.Database
{
    public class DbGoogleUser : IEquatable<DbGoogleUser?>
    {
        public string? Name { get; set; }
        public bool LocalGuide { get; set; }
        public int? NbReviews { get; set; }

        #region Equality

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DbGoogleUser);
        }

        public bool Equals(DbGoogleUser? obj)
        {
            return obj is DbGoogleUser user &&
                   Name == user.Name &&
                   LocalGuide == user.LocalGuide &&
                   NbReviews == user.NbReviews;
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, LocalGuide, NbReviews);
        }
        #endregion

        #region Local

        /// <summary>
        /// Create an instance of Google User.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nbReviews"></param>
        /// <param name="localGuide"></param>
        public DbGoogleUser(string? name, int? nbReviews, bool localGuide = false)
        {
            Name = name;
            LocalGuide = localGuide;
            NbReviews = nbReviews;
        }
        #endregion
    }
}