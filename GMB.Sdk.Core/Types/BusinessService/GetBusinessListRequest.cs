namespace GMB.Sdk.Core.Types.BusinessService
{
    #region Enum
    public enum CategoryFamily
    {
        UNIVERS,
        SECTEUR,
        ACTIVITE,
        VALEUR
    }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="processing"></param>
    /// <param name="category"></param>
    /// <param name="categoryFamily"></param>
    /// <param name="isNetwork"></param>
    /// <param name="isIndependant"></param>
    public class GetBusinessListRequest(int? entries, int? processing, string? brand, string? category, CategoryFamily? categoryFamily, bool isNetwork, bool isIndependant)
    {
        public int? Entries { get; set; } = entries;
        public int? Processing { get; set; } = processing;
        public string? Brand { get; set; } = brand;
        public string? Category { get; set; } = category;
        public CategoryFamily? CategoryFamily { get; set; } = categoryFamily;
        public bool IsNetwork { get; set; } = isNetwork;
        public bool IsIndependant { get; set; } = isIndependant;
    }
}
