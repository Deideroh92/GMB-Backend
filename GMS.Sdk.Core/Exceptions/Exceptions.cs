namespace GMS.BusinessProfile.Agent.Model {
    public class NoBusiness : Exception {

        #region Local

        public NoBusiness(string url)
            : base("No business on this google page " + url + "-> passing the business to deleted") {
        }
        #endregion
    }

    public class BusinessIncomplete : Exception {

        #region Local

        public BusinessIncomplete(string url)
            : base("Couldn't get all infos about this business : " + url) {
        }
        #endregion
    }
}
