namespace GMB.Sdk.Core.Types.Models
{
    public class GoogleUser
    {
        public string? Name { get; set; }
        public bool LocalGuide { get; set; }
        public int? NbReviews { get; set; }

        #region Local
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nbReviews"></param>
        /// <param name="localGuide"></param>
        public GoogleUser(string? name, int? nbReviews, bool localGuide = false)
        {
            Name = name;
            LocalGuide = localGuide;
            NbReviews = nbReviews;
        }
        #endregion
    }
}