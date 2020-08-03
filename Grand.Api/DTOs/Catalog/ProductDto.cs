using Grand.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductDto : BaseApiEntityModel
    {
        public ProductDto()
        {
            Categories = new List<ProductCategoryDto>();
            Manufacturers = new List<ProductManufacturerDto>();
            Pictures = new List<ProductPictureDto>();
            SpecificationAttribute = new List<ProductSpecificationAttributeDto>();
            TierPrices = new List<ProductTierPriceDto>();
            WarehouseInventory = new List<ProductWarehouseInventoryDto>();
            AttributeMappings = new List<ProductAttributeMappingDto>();
            AttributeCombinations = new List<ProductAttributeCombinationDto>();
            Tags = new List<string>();
            AppliedDiscounts = new List<string>();
        }
        [BsonElement("ProductTypeId")]
        public ProductType ProductType { get; set; }
        public string ParentGroupedProductId { get; set; }
        public bool VisibleIndividually { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public string ProductTemplateId { get; set; }
        public string VendorId { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public string ExternalId { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        [BsonElement("GiftCardTypeId")]
        public GiftCardType GiftCardType { get; set; }
        public decimal? OverriddenGiftCardAmount { get; set; }
        public bool RequireOtherProducts { get; set; }
        public string RequiredProductIds { get; set; }
        public bool AutomaticallyAddRequiredProducts { get; set; }
        public bool IsDownload { get; set; }
        public string DownloadId { get; set; }
        public bool UnlimitedDownloads { get; set; }
        [BsonElement("DownloadActivationTypeId")]
        public DownloadActivationType DownloadActivationType { get; set; }
        public int MaxNumberOfDownloads { get; set; }
        public int? DownloadExpirationDays { get; set; }
        public bool HasSampleDownload { get; set; }
        public string SampleDownloadId { get; set; }
        public bool HasUserAgreement { get; set; }
        public string UserAgreementText { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringCycleLength { get; set; }
        public int RecurringTotalCycles { get; set; }
        [BsonElement("RecurringCyclePeriodId")]
        public RecurringProductCyclePeriod RecurringCyclePeriod { get; set; }
        public bool IncBothDate { get; set; }
        public int Interval { get; set; }
        public IntervalUnit IntervalUnitType { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool ShipSeparately { get; set; }
        public decimal AdditionalShippingCharge { get; set; }
        public string DeliveryDateId { get; set; }
        public bool IsTaxExempt { get; set; }
        public string TaxCategoryId { get; set; }
        public bool IsTele { get; set; }
        public bool UseMultipleWarehouses { get; set; }
        public string WarehouseId { get; set; }
        public int StockQuantity { get; set; }
        [BsonElement("ManageInventoryMethodId")]
        public ManageInventoryMethod ManageInventoryMethod { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public bool DisplayStockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public bool LowStock { get; set; }
        [BsonElement("LowStockActivityId")]
        public LowStockActivity LowStockActivity { get; set; }
        public int NotifyAdminForQuantityBelow { get; set; }
        [BsonElement("BackorderModeId")]
        public BackorderMode BackorderMode { get; set; }
        public bool AllowBackInStockSubscriptions { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public string AllowedQuantities { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public bool NotReturnable { get; set; }
        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public bool AvailableForPreOrder { get; set; }
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
        public bool CallForPrice { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal CatalogPrice { get; set; }
        public decimal ProductCost { get; set; }
        public bool CustomerEntersPrice { get; set; }
        public decimal MinimumCustomerEnteredPrice { get; set; }
        public decimal MaximumCustomerEnteredPrice { get; set; }
        public bool BasepriceEnabled { get; set; }
        public decimal BasepriceAmount { get; set; }
        public string BasepriceUnitId { get; set; }
        public decimal BasepriceBaseAmount { get; set; }
        public string BasepriceBaseUnitId { get; set; }
        public string UnitId { get; set; }
        public bool MarkAsNew { get; set; }
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public decimal StartPrice { get; set; }
        public bool AuctionEnded { get; set; }
        public int DisplayOrder { get; set; }
        public int DisplayOrderCategory { get; set; }
        public int DisplayOrderManufacturer { get; set; }
        public bool Published { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int Sold { get; set; }
        public Int64 Viewed { get; set; }
        public int OnSale { get; set; }
        public string Flag { get; set; }
        [BsonElement("ProductCategories")]
        public IList<ProductCategoryDto> Categories { get; set; }
        [BsonElement("ProductManufacturers")]
        public IList<ProductManufacturerDto> Manufacturers { get; set; }
        [BsonElement("ProductPictures")]
        public IList<ProductPictureDto> Pictures { get; set; }
        [BsonElement("ProductSpecificationAttributes")]
        public IList<ProductSpecificationAttributeDto> SpecificationAttribute { get; set; }
        public IList<ProductTierPriceDto> TierPrices { get; set; }
        [BsonElement("ProductWarehouseInventory")]
        public IList<ProductWarehouseInventoryDto> WarehouseInventory { get; set; }
        [BsonElement("ProductAttributeMappings")]
        public IList<ProductAttributeMappingDto> AttributeMappings { get; set; }
        [BsonElement("ProductAttributeCombinations")]
        public IList<ProductAttributeCombinationDto> AttributeCombinations { get; set; }
        [BsonElement("ProductTags")]
        public IList<string> Tags { get; set; }
        public IList<string> AppliedDiscounts { get; set; }
    }
}
