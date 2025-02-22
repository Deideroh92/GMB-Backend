using GMB.Sdk.Core.FileGenerators.Sticker;

namespace GMB.Sdk.Core.Types.Database.Models
{
    public enum OrderStatus
    {
        Analyzing,
        Analyzed,
        Quoted,
        Ordered,
        Delivered,
        Error
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="createdAt"></param>
    /// <param name="updatedAt"></param>
    /// <param name="ownerId"></param>
    /// <param name="status"></param>
    /// <param name="language"></param>
    /// <param name="basePrice"></param>
    /// <param name="name"></param>
    /// <param name="vatAmount"></param>
    /// <param name="vatPrice"></param>
    /// <param name="discount"></param>
    /// <param name="priceNoVat"></param>
    /// <param name="priceWithVAT"></param>
    public class DbOrder(DateTime createdAt, DateTime updatedAt, string ownerId, OrderStatus status, StickerLanguage language, int basePrice, string name, int vatAmount, int vatPrice, int discount, int priceNoVat, int priceWithVAT)
    {
        public DateTime CreatedAt { get; set; } = createdAt;
        public DateTime UpdatedAt { get; set; } = updatedAt;
        public string OwnerId { get; set; } = ownerId;
        public OrderStatus Status { get; set; } = status;
        public StickerLanguage Language { get; set; } = language;
        public int BasePrice { get; set; } = basePrice;
        public int VatAmount { get; set; } = vatAmount;
        public int VatPrice { get; set; } = vatPrice;
        public int Discount { get; set; } = discount;
        public int PriceNoVat { get; set; } = priceNoVat;
        public int PriceWithVAT { get; set; } = priceWithVAT;
        public string Name { get; set; } = name;
    }
}
