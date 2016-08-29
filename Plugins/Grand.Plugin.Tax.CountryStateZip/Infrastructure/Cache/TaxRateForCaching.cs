namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure.Cache
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial class TaxRateForCaching
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string TaxCategoryId { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string Zip { get; set; }
        public decimal Percentage { get; set; }
    }
}