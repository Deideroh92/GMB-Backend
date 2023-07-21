
namespace GMS.Sdk.Core.Types.Models
{
    #region Enum
    public enum CategoryFamily {
        UNIVERS,
        SECTEUR,
        ACTIVITE,
        VALEUR
    }
    #endregion

    public class GetBusinessListRequest
    {
        public int? Entries { get; set; }
        public int? Processing { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public CategoryFamily? CategoryFamily { get; set; }
        public bool IsNetwork { get; set; }
        public bool IsIndependant { get; set; }
        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="processing"></param>
        /// <param name="category"></param>
        /// <param name="categoryFamily"></param>
        /// <param name="isNetwork"></param>
        /// <param name="isIndependant"></param>
        public GetBusinessListRequest(int? entries, int? processing, string? brand, string? category, CategoryFamily? categoryFamily, bool isNetwork, bool isIndependant)
        {
            Entries = entries;
            Processing = processing;
            Brand = brand;
            Category = category;
            CategoryFamily = categoryFamily;
            IsNetwork = isNetwork;
            IsIndependant = isIndependant;
        }
        #endregion
    }
}
