namespace GMB.Sdk.Core.Types.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="nbReviews"></param>
    /// <param name="localGuide"></param>
    public class GoogleUser(string? name, int? nbReviews, bool localGuide = false)
    {
        public string? Name { get; set; } = name;
        public bool LocalGuide { get; set; } = localGuide;
        public int? NbReviews { get; set; } = nbReviews;
    }
}