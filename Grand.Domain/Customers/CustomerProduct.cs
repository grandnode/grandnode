namespace Grand.Domain.Customers
{
    public class CustomerProduct: BaseEntity
    {
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
