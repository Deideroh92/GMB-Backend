﻿using GMB.Sdk.Core.FileGenerators.Sticker;
using GMB.Sdk.Core.Types.BusinessService;
using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.ScannerService
{
    #region Enum
    public enum Operation
    {
        URL_STATE,
        PROCESSING_STATE
    }

    public enum StickerType
    {
        PLACE_ID,
        COORDINATES
    }
    #endregion

    #region SubClasses
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public class StickerFileRowData(int id, string data)
    {
        public int Id { get; set; } = id;
        public string Data { get; set; } = data;
    }
    #endregion

    #region Requests
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="textSearch"></param>
    /// <param name="deptSearch"></param>
    /// <param name="cityCodeSearch"></param>
    public class ScannerUrlParameters(List<string>? locations, string textSearch, bool deptSearch = false, bool cityCodeSearch = false)
    {
        public bool DeptSearch { get; set; } = deptSearch;
        public bool CityCodeSearch { get; set; } = cityCodeSearch;
        public List<string>? Locations { get; set; } = locations;
        public string TextSearch { get; set; } = textSearch;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="getReviews"></param>
    /// <param name="businessList"></param>
    /// <param name="dateLimit"></param>
    /// <param name="updateProcessingState"></param>
    /// <param name="getPhotos"></param>
    public class ScannerBusinessParameters(Operation operation, bool getReviews, List<BusinessAgent>? businessList, DateTime? dateLimit = null, bool updateProcessingState = true, bool checkReviewStatus = false, bool getPhotos = false)
    {
        public List<BusinessAgent>? BusinessList { get; set; } = businessList;
        public bool GetReviews { get; set; } = getReviews;
        public bool UpdateProcessingState { get; set; } = updateProcessingState;
        public DateTime? DateLimit { get; set; } = dateLimit;
        public Operation Operation { get; set; } = operation;
        public bool CheckReviewStatus { get; set; } = checkReviewStatus;
        public bool GetPhotos { get; set; } = getPhotos;
    }
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="processing"></param>
    /// <param name="operationType"></param>
    /// <param name="getReviews"></param>
    /// <param name="reviewsDate"></param>
    /// <param name="isNetwork"></param>
    /// <param name="isIndependant"></param>
    /// <param name="categoryFamily"></param>
    /// <param name="brand"></param>
    /// <param name="category"></param>
    /// <param name="urlState"></param>
    /// <param name="updateProcessingState"></param>
    /// <param name="checkDeletedStatus"></param>
    /// <param name="checkPhotos"></param>
    public class BusinessScannerRequest(int? entries,
                                        int processing,
                                        Operation operationType,
                                        bool getReviews,
                                        DateTime reviewsDate,
                                        bool isNetwork = false,
                                        bool isIndependant = false,
                                        CategoryFamily? categoryFamily = null,
                                        string? brand = null,
                                        string? category = null,
                                        UrlState urlState = UrlState.NEW,
                                        bool updateProcessingState = true,
                                        bool checkDeletedStatus = false,
                                        bool checkPhotos = false)
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
        public bool UpdateProcessingState { get; set; } = updateProcessingState;
        public bool CheckDeletedStatus { get; set; } = checkDeletedStatus;
        public bool CheckPhotos { get; set; } = checkPhotos;
    }
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="places"></param>
    /// <param name="orderDate"></param>
    /// <param name="lang"></param>
    public class StickerScannerRequest(int orderId, List<DbPlace> places, DateTime orderDate, StickerLanguage lang)
    {
        public int OrderId { get; set; } = orderId;
        public StickerLanguage Lang { get; set; } = lang;
        public DateTime OrderDate { get; set; } = orderDate;
        public List<DbPlace> Places { get; set; } = places;
    }
    #endregion
}
