namespace GMS.Business.Api.Models
{
    #region Requests

    public class GetBusinessProfileRequest {
        public string Url { get; set; }
        public string? Guid { get; set; }
        public string? IdEtab { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="guid"></param>
        /// <param name="idEtab"></param>
        public GetBusinessProfileRequest(string url, string? guid = null, string? idEtab = null)
        {
            Url = url;
            Guid = guid;
            IdEtab = idEtab;
        }
        #endregion
    }
    #endregion
}
