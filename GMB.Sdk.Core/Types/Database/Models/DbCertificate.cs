namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="stickerId"></param>
    /// <param name="image"></param>
    public class DbCertificate(string stickerId, byte[] image)
    {
        public string StickerId { get; set; } = stickerId;
        public byte[] Image { get; set; } = image;
    }
}
