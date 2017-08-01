
namespace Grand.Services.Tax
{
    public class TaxProductPrice
    {
        public decimal UnitPriceInclTax { get; set; }
        public decimal UnitPriceExclTax { get; set; }
        public decimal SubTotalInclTax { get; set; }
        public decimal SubTotalExclTax { get; set; }
        public decimal discountAmountInclTax { get; set; }
        public decimal discountAmountExclTax { get; set; }
    }
}
