namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="score"></param>
    /// <param name="createdDate"></param>
    /// <param name="image"></param>
    /// <param name="certificate"></param>
    public class DbStickerNetwork(double score, DateTime createdDate, byte[]? image, byte[]? certificate, int nbEtab, int nbReview, int year, string brandName, string geoZone, int? id = null)
    {
        public int? Id { get; set; } = id;
        public double Score { get; set; } = score;
        public byte[]? Image { get; set; } = image;
        public DateTime CreatedDate { get; set; } = createdDate;
        public int NbEtab { get; set; } = nbEtab;
        public byte[]? Certificate { get; set; } = certificate;
        public int NbReview { get; set; } = nbReview;
        public int Year { get; set; } = year;
        public string BrandName { get; set; } = brandName;
        public string GeoZone { get; set; } = geoZone;
    }
}