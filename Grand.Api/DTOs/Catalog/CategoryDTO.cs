using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Catalog
{
    public partial class CategoryDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BottomDescription { get; set; }
        public string CategoryTemplateId { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public string ParentCategoryId { get; set; }
        public string PictureId { get; set; }
        public int PageSize { get; set; }
        public bool AllowCustomersToSelectPageSize { get; set; }
        public string PageSizeOptions { get; set; }
        public string PriceRanges { get; set; }
        public bool ShowOnHomePage { get; set; }
        public bool FeaturedProductsOnHomaPage { get; set; }
        public bool IncludeInTopMenu { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public string Flag { get; set; }
        public string FlagStyle { get; set; }
        public string Icon { get; set; }
        public bool HideOnCatalog { get; set; }
        public bool ShowOnSearchBox { get; set; }
        public int SearchBoxDisplayOrder { get; set; }
    }
}
