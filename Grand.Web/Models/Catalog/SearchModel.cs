using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class SearchModel : BaseGrandModel
    {
        public SearchModel()
        {
            PagingFilteringContext = new CatalogPagingFilteringModel();
            Products = new List<ProductOverviewModel>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        public string Warning { get; set; }

        public bool NoResults { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        [GrandResourceDisplayName("Search.SearchTerm")]
        public string q { get; set; }
        /// <summary>
        /// Category ID
        /// </summary>
        [GrandResourceDisplayName("Search.Category")]
        public string cid { get; set; }
        [GrandResourceDisplayName("Search.IncludeSubCategories")]
        public bool isc { get; set; }
        /// <summary>
        /// Manufacturer ID
        /// </summary>
        [GrandResourceDisplayName("Search.Manufacturer")]
        public string mid { get; set; }
        /// <summary>
        /// Vendor ID
        /// </summary>
        [GrandResourceDisplayName("Search.Vendor")]
        public string vid { get; set; }
        /// <summary>
        /// Price - From 
        /// </summary>
        [GrandResourceDisplayName("Search.PriceRange.From")]
        public string pf { get; set; }
        /// <summary>
        /// Price - To
        /// </summary>
        [GrandResourceDisplayName("Search.PriceRange.To")]
        public string pt { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [GrandResourceDisplayName("Search.SearchInDescriptions")]
        public bool sid { get; set; }
        /// <summary>
        /// A value indicating whether "advanced search" is enabled
        /// </summary>
        [GrandResourceDisplayName("Search.AdvancedSearch")]
        public bool adv { get; set; }
        /// <summary>
        /// A value indicating whether "allow search by vendor" is enabled
        /// </summary>
        public bool asv { get; set; }
        public bool Box { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }

        #region Nested classes

        public class CategoryModel : BaseGrandEntityModel
        {
            public string Breadcrumb { get; set; }
        }

        #endregion
    }
}