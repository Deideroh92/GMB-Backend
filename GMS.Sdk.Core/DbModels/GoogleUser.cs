namespace GMS.Sdk.Core.Database {
    public class GoogleUser : IEquatable<GoogleUser?> {
        public string? Name { get; set; }
        public bool LocalGuide { get; set; }
        public int? NbReviews { get; set; }

        #region Equality

        public override bool Equals(object? obj) {
            return Equals(obj as GoogleUser);
        }

        public bool Equals(GoogleUser? obj) {
            return obj is GoogleUser user &&
                   Name == user.Name &&
                   LocalGuide == user.LocalGuide &&
                   NbReviews == user.NbReviews;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Name, LocalGuide, NbReviews);
        }
        #endregion

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nbReviews"></param>
        /// <param name="localGuide"></param>
        public GoogleUser(string? name, int? nbReviews, bool localGuide = false) {
            Name = name;
            LocalGuide = localGuide;
            NbReviews = nbReviews;
        }
        #endregion
    }
}