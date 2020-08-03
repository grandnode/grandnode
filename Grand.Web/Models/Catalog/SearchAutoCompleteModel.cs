namespace Grand.Web.Models.Catalog
{
    public class SearchAutoCompleteModel
    {
        public string SearchType { get; set; }
        public string Label { get; set; }
        public string Url { get; set; }
        public string Desc { get; set; }
        public string Price { get; set; }
        public string PriceWithDiscount { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int Rating { get; set; }
        public string PictureUrl { get; set; }
    }
}
