using GMB.Scanner.Agent.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class BusinessScannerRequest(int? entries, int processing, Operation operationType, bool getReviews, DateTime reviewsDate, bool isNetwork = false, bool isIndependant = false, CategoryFamily? categoryFamily = null, string? brand = null, string? category = null, UrlState urlState = UrlState.NEW)
    {
        public int? Entries { get; set; } = entries;
        public int Processing { get; set; } = processing;
        public Operation OperationType { get; set; } = operationType;
        public bool GetReviews { get; set; } = getReviews;
        public DateTime ReviewsDate { get; set; } = reviewsDate;
        public bool IsNetwork { get; set; } = isNetwork;
        public bool IsIndependant { get; set; } = isIndependant;
        public CategoryFamily? CategoryFamily { get; set; } = categoryFamily;
        public string? Brand { get; set; } = brand;
        public string? Category { get; set; } = category;
        public UrlState UrlState { get; set; } = urlState;
    }
}