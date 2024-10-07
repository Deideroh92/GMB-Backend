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

    public enum Languages
    {
        EN,
        FR,
        ES,
        IT,
        DE
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="createdAt"></param>
    /// <param name="updatedAt"></param>
    /// <param name="ownerId"></param>
    /// <param name="status"></param>
    /// <param name="language"></param>
    /// <param name="price"></param>
    /// <param name="name"></param>
    public class DbOrder(DateTime createdAt, DateTime updatedAt, string ownerId, OrderStatus status, Languages language, int price, string name)
    {
        public DateTime CreatedAt { get; set; } = createdAt;
        public DateTime UpdatedAt { get; set; } = updatedAt;
        public string OwnerId { get; set; } = ownerId;
        public OrderStatus Status { get; set; } = status;
        public Languages Language { get; set; } = language;
        public int Price { get; set; } = price;
        public string Name { get; set; } = name;
    }
}
