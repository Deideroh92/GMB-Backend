namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="stickerId"></param>
    /// <param name="image"></param>
    public class DbCertificate(int stickerId, byte[] image)
    {
        public int StickerId { get; set; } = stickerId;
        public byte[] Image { get; set; } = image;
    }
}
