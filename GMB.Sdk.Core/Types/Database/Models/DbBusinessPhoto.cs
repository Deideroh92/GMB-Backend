namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="idEtab"></param>
    /// <param name="photoUrl"></param>
    /// <param name="isOwner"></param>
    /// <param name="dateInsert"></param>
    public class DbBusinessPhoto(string idEtab, string? photoUrl, bool isOwner, DateTime? dateInsert = null)
    {
        public long Id { get; set; }
        public string IdEtab { get; set; } = idEtab;
        public string? PhotoUrl { get; set; } = photoUrl;
        public bool IsOwner { get; set; } = isOwner;
        public DateTime? DateInsert { get; set; } = dateInsert ?? DateTime.Now;
    }
}
