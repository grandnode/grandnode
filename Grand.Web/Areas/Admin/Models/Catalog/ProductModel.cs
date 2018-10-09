using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(ProductValidator))]
    public partial class ProductModel : BaseGrandEntityModel, ILocalizedModel<ProductLocalizedModel>
    {
        public ProductModel()
        {
            Locales = new List<ProductLocalizedModel>();
            ProductPictureModels = new List<ProductPictureModel>();
            CopyProductModel = new CopyProductModel();
            AvailableBasepriceUnits = new List<SelectListItem>();
            AvailableBasepriceBaseUnits = new List<SelectListItem>();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableDeliveryDates = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductAttributes = new List<SelectListItem>();
            AvailableUnits = new List<SelectListItem>();
            AddPictureModel = new ProductPictureModel();
            AddSpecificationAttributeModel = new AddProductSpecificationAttributeModel();
            ProductWarehouseInventoryModels = new List<ProductWarehouseInventoryModel>();
            CalendarModel = new GenerateCalendarModel();
        }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ID")]
        public override string Id { get; set; }

        //picture thumbnail
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
        public int ProductTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
        public string ProductTypeName { get; set; }
        public bool AuctionEnded { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AssociatedToProductName")]
        public string AssociatedToProductId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AssociatedToProductName")]
        public string AssociatedToProductName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.VisibleIndividually")]
        public bool VisibleIndividually { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ProductTemplate")]
        public string ProductTemplateId { get; set; }
        public IList<SelectListItem> AvailableProductTemplates { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
        public string FullDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Flag")]
        public string Flag { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AdminComment")]
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Vendor")]
        public string VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.SeName")]
        
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ProductTags")]
        public string ProductTags { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        
        public string Sku { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ManufacturerPartNumber")]
        
        public string ManufacturerPartNumber { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.GTIN")]
        
        public virtual string Gtin { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsGiftCard")]
        public bool IsGiftCard { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.GiftCardType")]
        public int GiftCardTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.OverriddenGiftCardAmount")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenGiftCardAmount { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.RequireOtherProducts")]
        public bool RequireOtherProducts { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.RequiredProductIds")]
        public string RequiredProductIds { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AutomaticallyAddRequiredProducts")]
        public bool AutomaticallyAddRequiredProducts { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsDownload")]
        public bool IsDownload { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Download")]
        [UIHint("Download")]
        public string DownloadId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.UnlimitedDownloads")]
        public bool UnlimitedDownloads { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MaxNumberOfDownloads")]
        public int MaxNumberOfDownloads { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DownloadExpirationDays")]
        [UIHint("Int32Nullable")]
        public int? DownloadExpirationDays { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DownloadActivationType")]
        public int DownloadActivationTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.HasSampleDownload")]
        public bool HasSampleDownload { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.SampleDownload")]
        [UIHint("Download")]
        public string SampleDownloadId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.HasUserAgreement")]
        public bool HasUserAgreement { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.UserAgreementText")]
        
        public string UserAgreementText { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsRecurring")]
        public bool IsRecurring { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.RecurringCycleLength")]
        public int RecurringCycleLength { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.RecurringCyclePeriod")]
        public int RecurringCyclePeriodId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.RecurringTotalCycles")]
        public int RecurringTotalCycles { get; set; }

        //calendar
        public GenerateCalendarModel CalendarModel { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsShipEnabled")]
        public bool IsShipEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsFreeShipping")]
        public bool IsFreeShipping { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ShipSeparately")]
        public bool ShipSeparately { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AdditionalShippingCharge")]
        public decimal AdditionalShippingCharge { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DeliveryDate")]
        public string DeliveryDateId { get; set; }
        public IList<SelectListItem> AvailableDeliveryDates { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.TaxCategory")]
        public string TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.IsTelecommunicationsOrBroadcastingOrElectronicServices")]
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ManageInventoryMethod")]
        public int ManageInventoryMethodId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.UseMultipleWarehouses")]
        public bool UseMultipleWarehouses { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Warehouse")]
        public string WarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
        public int StockQuantity { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
        public string StockQuantityStr { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayStockAvailability")]
        public bool DisplayStockAvailability { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayStockQuantity")]
        public bool DisplayStockQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MinStockQuantity")]
        public int MinStockQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.LowStockActivity")]
        public int LowStockActivityId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.NotifyAdminForQuantityBelow")]
        public int NotifyAdminForQuantityBelow { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BackorderMode")]
        public int BackorderModeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AllowBackInStockSubscriptions")]
        public bool AllowBackInStockSubscriptions { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.OrderMinimumQuantity")]
        public int OrderMinimumQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.OrderMaximumQuantity")]
        public int OrderMaximumQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AllowedQuantities")]
        public string AllowedQuantities { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AllowAddingOnlyExistingAttributeCombinations")]
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.NotReturnable")]
        public bool NotReturnable { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisableBuyButton")]
        public bool DisableBuyButton { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisableWishlistButton")]
        public bool DisableWishlistButton { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AvailableForPreOrder")]
        public bool AvailableForPreOrder { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.PreOrderAvailabilityStartDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.CallForPrice")]
        public bool CallForPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.CatalogPrice")]
        public decimal CatalogPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.StartPrice")]
        public decimal StartPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ProductCost")]
        public decimal ProductCost { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.CustomerEntersPrice")]
        public bool CustomerEntersPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MinimumCustomerEnteredPrice")]
        public decimal MinimumCustomerEnteredPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MaximumCustomerEnteredPrice")]
        public decimal MaximumCustomerEnteredPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BasepriceEnabled")]
        public bool BasepriceEnabled { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BasepriceAmount")]
        public decimal BasepriceAmount { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BasepriceUnit")]
        public string BasepriceUnitId { get; set; }
        public IList<SelectListItem> AvailableBasepriceUnits { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BasepriceBaseAmount")]
        public decimal BasepriceBaseAmount { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.BasepriceBaseUnit")]
        public string BasepriceBaseUnitId { get; set; }
        public IList<SelectListItem> AvailableBasepriceBaseUnits { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MarkAsNew")]
        public bool MarkAsNew { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MarkAsNewStartDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MarkAsNewEndDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Unit")]
        public string UnitId { get; set; }
        public IList<SelectListItem> AvailableUnits { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Weight")]
        public decimal Weight { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Length")]
        public decimal Length { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Width")]
        public decimal Width { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Height")]
        public decimal Height { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AvailableStartDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableStartDateTimeUtc { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AvailableEndDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableEndDateTimeUtc { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayOrderCategory")]
        public int DisplayOrderCategory { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayOrderManufacturer")]
        public int DisplayOrderManufacturer { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.DisplayOrderOnSale")]
        public int OnSale { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.CreatedOn")]
        public DateTime? CreatedOn { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }
        public long Ticks { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseDimensionIn { get; set; }
        public string BaseWeightIn { get; set; }

        public IList<ProductLocalizedModel> Locales { get; set; }


        //ACL (customer roles)
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


        //vendor
        public bool IsLoggedInAsVendor { get; set; }



        //categories
        public IList<SelectListItem> AvailableCategories { get; set; }
        //manufacturers
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        //product attributes
        public IList<SelectListItem> AvailableProductAttributes { get; set; }
        


        //pictures
        public ProductPictureModel AddPictureModel { get; set; }
        public IList<ProductPictureModel> ProductPictureModels { get; set; }

        //discounts
        public List<DiscountModel> AvailableDiscounts { get; set; }
        public string[] SelectedDiscountIds { get; set; }




        //add specification attribute model
        public AddProductSpecificationAttributeModel AddSpecificationAttributeModel { get; set; }


        //multiple warehouses
        [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory")]
        public IList<ProductWarehouseInventoryModel> ProductWarehouseInventoryModels { get; set; }

        //copy product
        public CopyProductModel CopyProductModel { get; set; }
        
        #region Nested classes

        public partial class AddRequiredProductModel : BaseGrandModel
        {
            public AddRequiredProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            
            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class AddProductSpecificationAttributeModel : BaseGrandModel
        {
            public AddProductSpecificationAttributeModel()
            {
                AvailableAttributes = new List<SelectListItem>();
                AvailableOptions = new List<SelectListItem>();
            }
            
            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
            public string SpecificationAttributeId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AttributeType")]
            public int AttributeTypeId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttributeOption")]
            public string SpecificationAttributeOptionId { get; set; }

            
            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.CustomValue")]
            public string CustomValue { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AllowFiltering")]
            public bool AllowFiltering { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage")]
            public bool ShowOnProductPage { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public IList<SelectListItem> AvailableAttributes { get; set; }
            public IList<SelectListItem> AvailableOptions { get; set; }
        }
        
        public partial class ProductPictureModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }

            [UIHint("Picture")]
            [GrandResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public string PictureId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public string PictureUrl { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.OverrideAltAttribute")]
            
            public string OverrideAltAttribute { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.OverrideTitleAttribute")]
            
            public string OverrideTitleAttribute { get; set; }
        }
        
        public partial class ProductCategoryModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Products.Categories.Fields.Category")]
            public string Category { get; set; }

            public string ProductId { get; set; }

            public string CategoryId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Categories.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Categories.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class ProductManufacturerModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.Manufacturer")]
            public string Manufacturer { get; set; }

            public string ProductId { get; set; }

            public string ManufacturerId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class RelatedProductModel : BaseGrandEntityModel
        {
            public string ProductId1 { get; set; }
            public string ProductId2 { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.RelatedProducts.Fields.Product")]
            public string Product2Name { get; set; }
            
            [GrandResourceDisplayName("Admin.Catalog.Products.RelatedProducts.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }
        public partial class AddRelatedProductModel : BaseGrandModel
        {
            public AddRelatedProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            
            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public string ProductId { get; set; }

            public string[] SelectedProductIds { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class BundleProductModel : BaseGrandEntityModel
        {
            public string ProductBundleId { get; set; }
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.BundleProducts.Fields.Product")]
            public string ProductName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.BundleProducts.Fields.Quantity")]
            public int Quantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.BundleProducts.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }
        public partial class AddBundleProductModel : BaseGrandModel
        {
            public AddBundleProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]

            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }

            public string ProductId { get; set; }

            public string[] SelectedProductIds { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }


        public partial class AssociatedProductModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Products.AssociatedProducts.Fields.Product")]
            public string ProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.AssociatedProducts.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }
        public partial class AddAssociatedProductModel : BaseGrandModel
        {
            public AddAssociatedProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            
            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public string ProductId { get; set; }

            public string[] SelectedProductIds { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class CrossSellProductModel : BaseGrandEntityModel
        {
            public string ProductId2 { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.CrossSells.Fields.Product")]
            public string Product2Name { get; set; }
        }
        public partial class AddCrossSellProductModel : BaseGrandModel
        {
            public AddCrossSellProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            
            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public string SearchStoreId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public string SearchVendorId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public string ProductId { get; set; }

            public string[] SelectedProductIds { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class TierPriceModel : BaseGrandEntityModel
        {

            public TierPriceModel()
            {
                AvailableStores = new List<SelectListItem>();
                AvailableCustomerRoles = new List<SelectListItem>();
            }
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.CustomerRole")]
            public string CustomerRoleId { get; set; }
            public IList<SelectListItem> AvailableCustomerRoles { get; set; }
            public string CustomerRole { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Store")]
            public string StoreId { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public string Store { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Quantity")]
            public int Quantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Price")]
            public decimal Price { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.StartDateTimeUtc")]
            [UIHint("DateTimeNullable")]
            public DateTime? StartDateTimeUtc { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.EndDateTimeUtc")]
            [UIHint("DateTimeNullable")]
            public DateTime? EndDateTimeUtc { get; set; }

        }

        public partial class ProductWarehouseInventoryModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.Warehouse")]
            public string WarehouseId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.Warehouse")]
            public string WarehouseName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.WarehouseUsed")]
            public bool WarehouseUsed { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.StockQuantity")]
            public int StockQuantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.ReservedQuantity")]
            public int ReservedQuantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductWarehouseInventory.Fields.PlannedQuantity")]
            public int PlannedQuantity { get; set; }
        }
        public partial class ReservationModel : BaseGrandEntityModel
        {
            public string ReservationId { get; set; }
            public DateTime Date { get; set; }
            public string Resource { get; set; }
            public string Parameter { get; set; }
            public string OrderId { get; set; }
            public string Duration { get; set; }
        }

        public partial class BidModel : BaseGrandEntityModel
        {
            public string BidId { get; set; }
            public DateTime Date { get; set; }
            public string CustomerId { get; set; }
            public string Email { get; set; }
            public string Amount { get; set; }
            public string OrderId { get; set; }
        }

        [Validator(typeof(GenerateCalendarValidator))]
        public partial class GenerateCalendarModel : BaseGrandModel
        {

            public GenerateCalendarModel()
            {
                Interval = 1;
                Quantity = 1;
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.StartDateUtc")]
            [UIHint("Date")]
            public DateTime StartDateUtc { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.StartTime")]
            [UIHint("Time")]
            public DateTime StartTime { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.EndDateUtc")]
            [UIHint("Date")]
            public DateTime EndDateUtc { get; set; }
            [UIHint("Time")]

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.EndTime")]
            public DateTime EndTime { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Interval")]
            public int Interval { get; set; }
            public int IntervalUnit { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.IncBothDate")]
            public bool IncBothDate { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Quantity")]
            public int Quantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Resource")]
            public string Resource { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Parameter")]
            public string Parameter { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Monday")]
            public bool Monday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Tuesday")]
            public bool Tuesday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Wednesday")]
            public bool Wednesday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Thursday")]
            public bool Thursday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Friday")]
            public bool Friday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Saturday")]
            public bool Saturday { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Calendar.Sunday")]
            public bool Sunday { get; set; }

        }
        public partial class ProductAttributeMappingModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }

            public string ProductAttributeId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.Attribute")]
            public string ProductAttribute { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.TextPrompt")]
            
            public string TextPrompt { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.IsRequired")]
            public bool IsRequired { get; set; }

            public int AttributeControlTypeId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.AttributeControlType")]
            public string AttributeControlType { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public bool ShouldHaveValues { get; set; }
            public int TotalValues { get; set; }

            //validation fields
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules")]
            public bool ValidationRulesAllowed { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MinLength")]
            [UIHint("Int32Nullable")]
            public int? ValidationMinLength { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MaxLength")]
            [UIHint("Int32Nullable")]
            public int? ValidationMaxLength { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileAllowedExtensions")]
            
            public string ValidationFileAllowedExtensions { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileMaximumSize")]
            [UIHint("Int32Nullable")]
            public int? ValidationFileMaximumSize { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.DefaultValue")]
            
            public string DefaultValue { get; set; }
            public string ValidationRulesString { get; set; }

            //condition
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Condition")]
            public bool ConditionAllowed { get; set; }
            public string ConditionString { get; set; }
        }
        public partial class ProductAttributeValueListModel : BaseGrandModel
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductAttributeMappingId { get; set; }

            public string ProductAttributeName { get; set; }
        }
        [Validator(typeof(ProductAttributeValueModelValidator))]
        public partial class ProductAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<ProductAttributeValueLocalizedModel>
        {
            public ProductAttributeValueModel()
            {
                ProductPictureModels = new List<ProductPictureModel>();
                Locales = new List<ProductAttributeValueLocalizedModel>();
            }

            public string ProductAttributeMappingId { get; set; }
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AttributeValueType")]
            public int AttributeValueTypeId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AttributeValueType")]
            public string AttributeValueTypeName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct")]
            public string AssociatedProductId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct")]
            public string AssociatedProductName { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Name")]
            
            public string Name { get; set; }
            
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.ColorSquaresRgb")]
            
            public string ColorSquaresRgb { get; set; }
            public bool DisplayColorSquaresRgb { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.ImageSquaresPicture")]
            [UIHint("Picture")]
            public string ImageSquaresPictureId { get; set; }
            public bool DisplayImageSquaresPicture { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.PriceAdjustment")]
            public decimal PriceAdjustment { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.PriceAdjustment")]
            //used only on the values list page
            public string PriceAdjustmentStr { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.WeightAdjustment")]
            public decimal WeightAdjustment { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.WeightAdjustment")]
            //used only on the values list page
            public string WeightAdjustmentStr { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Cost")]
            public decimal Cost { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Quantity")]
            public int Quantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.IsPreSelected")]
            public bool IsPreSelected { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Picture")]
            public string PictureId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Picture")]
            public string PictureThumbnailUrl { get; set; }

            public IList<ProductPictureModel> ProductPictureModels { get; set; }
            public IList<ProductAttributeValueLocalizedModel> Locales { get; set; }

            #region Nested classes

            public partial class AssociateProductToAttributeValueModel : BaseGrandModel
            {
                public AssociateProductToAttributeValueModel()
                {
                    AvailableCategories = new List<SelectListItem>();
                    AvailableManufacturers = new List<SelectListItem>();
                    AvailableStores = new List<SelectListItem>();
                    AvailableVendors = new List<SelectListItem>();
                    AvailableProductTypes = new List<SelectListItem>();
                }

                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
                
                public string SearchProductName { get; set; }
                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
                public string SearchCategoryId { get; set; }
                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
                public string SearchManufacturerId { get; set; }
                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
                public string SearchStoreId { get; set; }
                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
                public string SearchVendorId { get; set; }
                [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
                public int SearchProductTypeId { get; set; }

                public IList<SelectListItem> AvailableCategories { get; set; }
                public IList<SelectListItem> AvailableManufacturers { get; set; }
                public IList<SelectListItem> AvailableStores { get; set; }
                public IList<SelectListItem> AvailableVendors { get; set; }
                public IList<SelectListItem> AvailableProductTypes { get; set; }
                
                //vendor
                public bool IsLoggedInAsVendor { get; set; }


                public string AssociatedToProductId { get; set; }
            }


            #endregion
        }
        public partial class ActivityLogModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Products.ActivityLog.ActivityLogType")]
            public string ActivityLogTypeName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ActivityLog.Comment")]
            public string Comment { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ActivityLog.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.ActivityLog.Customer")]
            public string CustomerId { get; set; }
            public string CustomerEmail { get; set; }
        }
        public partial class ProductAttributeValueLocalizedModel : ILocalizedModelLocal
        {
            public string LanguageId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.Name")]
            
            public string Name { get; set; }
        }
        public partial class ProductAttributeCombinationModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Attributes")]
            
            public string AttributesXml { get; set; }

            
            public string Warnings { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.StockQuantity")]
            public int StockQuantity { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders")]
            public bool AllowOutOfStockOrders { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Sku")]
            public string Sku { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.ManufacturerPartNumber")]
            public string ManufacturerPartNumber { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Gtin")]
            public string Gtin { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.OverriddenPrice")]
            [UIHint("DecimalNullable")]
            public decimal? OverriddenPrice { get; set; }

            [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.NotifyAdminForQuantityBelow")]
            public int NotifyAdminForQuantityBelow { get; set; }

        }
        public partial class ProductAttributeCombinationTierPricesModel : BaseGrandEntityModel
        {
            public string StoreId { get; set; }
            public string Store { get; set; }

            /// <summary>
            /// Gets or sets the customer role identifier
            /// </summary>
            public string CustomerRoleId { get; set; }
            public string CustomerRole { get; set; }

            /// <summary>
            /// Gets or sets the quantity
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// Gets or sets the price
            /// </summary>
            public decimal Price { get; set; }
        }

        #endregion
    }

    public partial class ProductLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
        
        public string ShortDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
        
        public string FullDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.SeName")]
        
        public string SeName { get; set; }
    }
}