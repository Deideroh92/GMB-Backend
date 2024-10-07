namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="placeId"></param>
    /// <param name="score"></param>
    /// <param name="createdDate"></param>
    /// <param name="image"></param>
    /// <param name="certificate"></param>
    /// <param name="orderId"></param>
    /// <param name="nbRating1"></param>
    /// <param name="nbRating2"></param>
    /// <param name="nbRating3"></param>
    /// <param name="nbRating4"></param>
    /// <param name="nbRating5"></param>
    public class DbSticker(string placeId, decimal score, DateTime createdDate, byte[] image, byte[] certificate, int orderId, int nbRating1, int nbRating2, int nbRating3, int nbRating4, int nbRating5, int? id = null)
    {
        public int? Id { get; set; } = id;
        public string PlaceId { get; set; } = placeId;
        public decimal Score { get; set; } = score;
        public byte[] Image { get; set; } = image;
        public DateTime CreatedDate { get; set; } = createdDate;
        public int OrderId { get; set; } = orderId;
        public byte[] Certificate { get; set; } = certificate;
        public int NbRating1 { get; set; } = nbRating1;
        public int NbRating2 { get; set; } = nbRating2;
        public int NbRating3 { get; set; } = nbRating3;
        public int NbRating4 { get; set; } = nbRating4;
        public int NbRating5 { get; set; } = nbRating5;
    }
}