using Grand.Core;
using Grand.Core.Extensions;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Logging;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.ExportImport.Help;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreService _storeService;
        private readonly IDiscountService _discountService;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreService storeService,
            IProductService productService,
            IDiscountService discountService,
            IServiceProvider serviceProvider)
        {
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productAttributeService = productAttributeService;
            _pictureService = pictureService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeService = storeService;
            _productService = productService;
            _discountService = discountService;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Utilities

        protected virtual async Task WriteCategories(XmlWriter xmlWriter, List<Category> categories)
        {
            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    xmlWriter.WriteStartElement("Category");
                    xmlWriter.WriteElementString("Id", null, category.Id);
                    xmlWriter.WriteElementString("Name", null, category.Name);
                    xmlWriter.WriteElementString("Description", null, category.Description);
                    xmlWriter.WriteElementString("CategoryTemplateId", null, category.CategoryTemplateId);
                    xmlWriter.WriteElementString("MetaKeywords", null, category.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, category.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, category.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, category.GetSeName(""));
                    xmlWriter.WriteElementString("ParentCategoryId", null, category.ParentCategoryId);
                    xmlWriter.WriteElementString("PictureId", null, category.PictureId);
                    xmlWriter.WriteElementString("PageSize", null, category.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, category.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, category.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, category.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, category.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("IncludeInTopMenu", null, category.IncludeInTopMenu.ToString());
                    xmlWriter.WriteElementString("Published", null, category.Published.ToString());
                    xmlWriter.WriteElementString("Flag", null, category.Flag);
                    xmlWriter.WriteElementString("FlagStyle", null, category.FlagStyle);
                    xmlWriter.WriteElementString("Icon", null, category.Icon);
                    xmlWriter.WriteElementString("DisplayOrder", null, category.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, category.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, category.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productCategories = await _categoryService.GetProductCategoriesByCategoryId(category.Id, showHidden: true);
                    foreach (var productCategory in productCategories)
                    {
                        var cat = await _categoryService.GetCategoryById(productCategory.CategoryId);
                        if (cat != null)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteElementString("ProductCategoryId", null, cat.Id);
                            xmlWriter.WriteElementString("ProductId", null, productCategory.ProductId);
                            xmlWriter.WriteElementString("ProductName", null, cat.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportManufacturersToXml(IList<Manufacturer> manufacturers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true,
            };
            var xmlWriter = XmlWriter.Create(stringWriter, xwSettings);
            await xmlWriter.WriteStartDocumentAsync();
            xmlWriter.WriteStartElement("Manufacturers");
            xmlWriter.WriteAttributeString("Version", GrandVersion.FullVersion);

            foreach (var manufacturer in manufacturers)
            {
                xmlWriter.WriteStartElement("Manufacturer");

                xmlWriter.WriteElementString("ManufacturerId", null, manufacturer.Id.ToString());
                xmlWriter.WriteElementString("Name", null, manufacturer.Name);
                xmlWriter.WriteElementString("Description", null, manufacturer.Description);
                xmlWriter.WriteElementString("ManufacturerTemplateId", null, manufacturer.ManufacturerTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, manufacturer.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, manufacturer.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, manufacturer.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, manufacturer.GetSeName(""));
                xmlWriter.WriteElementString("PictureId", null, manufacturer.PictureId);
                xmlWriter.WriteElementString("PageSize", null, manufacturer.PageSize.ToString());
                xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, manufacturer.AllowCustomersToSelectPageSize.ToString());
                xmlWriter.WriteElementString("PageSizeOptions", null, manufacturer.PageSizeOptions);
                xmlWriter.WriteElementString("PriceRanges", null, manufacturer.PriceRanges);
                xmlWriter.WriteElementString("Published", null, manufacturer.Published.ToString());
                xmlWriter.WriteElementString("DisplayOrder", null, manufacturer.DisplayOrder.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, manufacturer.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, manufacturer.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("Products");
                var productManufacturers = await _manufacturerService.GetProductManufacturersByManufacturerId(manufacturer.Id, "", showHidden: true);
                if (productManufacturers != null)
                {
                    var productService = _serviceProvider.GetRequiredService<IProductService>();
                    foreach (var productManufacturer in productManufacturers)
                    {
                        var product = await productService.GetProductById(productManufacturer.ProductId);
                        if (product != null)
                        {
                            xmlWriter.WriteStartElement("ProductManufacturer");
                            xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productManufacturer.ProductId.ToString());
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            await xmlWriter.FlushAsync();
            return stringWriter.ToString();
        }


        /// <summary>
        /// Export manufacturers to XLSX
        /// </summary>
        /// <param name="manufacturers">Manufactures</param>
        public virtual byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<Manufacturer>("Id", p => p.Id),
                new PropertyByName<Manufacturer>("Name", p => p.Name),
                new PropertyByName<Manufacturer>("Description", p => p.Description),
                new PropertyByName<Manufacturer>("ManufacturerTemplateId", p => p.ManufacturerTemplateId),
                new PropertyByName<Manufacturer>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Manufacturer>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Manufacturer>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Manufacturer>("SeName", p => p.SeName),
                new PropertyByName<Manufacturer>("Picture", p => GetPictures(p.PictureId).Result),
                new PropertyByName<Manufacturer>("PageSize", p => p.PageSize),
                new PropertyByName<Manufacturer>("AllowCustomersToSelectPageSize", p => p.AllowCustomersToSelectPageSize),
                new PropertyByName<Manufacturer>("PageSizeOptions", p => p.PageSizeOptions),
                new PropertyByName<Manufacturer>("PriceRanges", p => p.PriceRanges),
                new PropertyByName<Manufacturer>("Published", p => p.Published),
                new PropertyByName<Manufacturer>("DisplayOrder", p => p.DisplayOrder)
            };

            return ExportToXlsx(properties, manufacturers);
        }
        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportCategoriesToXml(IEnumerable<Category> categories)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true,
            };
            var xmlWriter = XmlWriter.Create(stringWriter, xwSettings);
            await xmlWriter.WriteStartDocumentAsync();
            xmlWriter.WriteStartElement("Categories");
            xmlWriter.WriteAttributeString("Version", GrandVersion.FullVersion);
            await WriteCategories(xmlWriter, categories.ToList());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            await xmlWriter.FlushAsync();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        public virtual byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<Category>("Id", p => p.Id),
                new PropertyByName<Category>("Name", p => p.Name),
                new PropertyByName<Category>("Description", p => p.Description),
                new PropertyByName<Category>("CategoryTemplateId", p => p.CategoryTemplateId),
                new PropertyByName<Category>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Category>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Category>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Category>("SeName", p => p.SeName),
                new PropertyByName<Category>("ParentCategoryId", p => p.ParentCategoryId),
                new PropertyByName<Category>("Picture", p => GetPictures(p.PictureId).Result),
                new PropertyByName<Category>("PageSize", p => p.PageSize),
                new PropertyByName<Category>("AllowCustomersToSelectPageSize", p => p.AllowCustomersToSelectPageSize),
                new PropertyByName<Category>("PageSizeOptions", p => p.PageSizeOptions),
                new PropertyByName<Category>("PriceRanges", p => p.PriceRanges),
                new PropertyByName<Category>("ShowOnHomePage", p => p.ShowOnHomePage),
                new PropertyByName<Category>("IncludeInTopMenu", p => p.IncludeInTopMenu),
                new PropertyByName<Category>("Published", p => p.Published),
                new PropertyByName<Category>("Flag", p => p.Flag),
                new PropertyByName<Category>("FlagStyle", p => p.FlagStyle),
                new PropertyByName<Category>("Icon", p => p.Icon),
                new PropertyByName<Category>("DisplayOrder", p => p.DisplayOrder)
            };
            return ExportToXlsx(properties, categories);
        }
        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportProductsToXml(IList<Product> products)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true
            };
            var xmlWriter = XmlWriter.Create(stringWriter, xwSettings);
            await xmlWriter.WriteStartDocumentAsync();
            xmlWriter.WriteStartElement("Products");
            xmlWriter.WriteAttributeString("Version", GrandVersion.FullVersion);

            foreach (var product in products)
            {
                xmlWriter.WriteStartElement("Product");

                xmlWriter.WriteElementString("ProductId", null, product.Id);
                xmlWriter.WriteElementString("ProductTypeId", null, product.ProductTypeId.ToString());
                xmlWriter.WriteElementString("ParentGroupedProductId", null, product.ParentGroupedProductId);
                xmlWriter.WriteElementString("VisibleIndividually", null, product.VisibleIndividually.ToString());
                xmlWriter.WriteElementString("Name", null, product.Name);
                xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                xmlWriter.WriteElementString("Flag", null, product.Flag);
                xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                xmlWriter.WriteElementString("VendorId", null, product.VendorId);
                xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId);
                xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, product.GetSeName(""));
                xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                xmlWriter.WriteElementString("SKU", null, product.Sku);
                xmlWriter.WriteElementString("ManufacturerPartNumber", null, product.ManufacturerPartNumber);
                xmlWriter.WriteElementString("Gtin", null, product.Gtin);
                xmlWriter.WriteElementString("IsGiftCard", null, product.IsGiftCard.ToString());
                xmlWriter.WriteElementString("GiftCardType", null, product.GiftCardType.ToString());
                xmlWriter.WriteElementString("OverriddenGiftCardAmount", null, product.OverriddenGiftCardAmount.HasValue ? product.OverriddenGiftCardAmount.ToString() : "");
                xmlWriter.WriteElementString("RequireOtherProducts", null, product.RequireOtherProducts.ToString());
                xmlWriter.WriteElementString("RequiredProductIds", null, product.RequiredProductIds);
                xmlWriter.WriteElementString("AutomaticallyAddRequiredProducts", null, product.AutomaticallyAddRequiredProducts.ToString());
                xmlWriter.WriteElementString("IsDownload", null, product.IsDownload.ToString());
                xmlWriter.WriteElementString("DownloadId", null, product.DownloadId);
                xmlWriter.WriteElementString("UnlimitedDownloads", null, product.UnlimitedDownloads.ToString());
                xmlWriter.WriteElementString("MaxNumberOfDownloads", null, product.MaxNumberOfDownloads.ToString());
                if (product.DownloadExpirationDays.HasValue)
                    xmlWriter.WriteElementString("DownloadExpirationDays", null, product.DownloadExpirationDays.ToString());
                else
                    xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
                xmlWriter.WriteElementString("DownloadActivationType", null, product.DownloadActivationType.ToString());
                xmlWriter.WriteElementString("HasSampleDownload", null, product.HasSampleDownload.ToString());
                xmlWriter.WriteElementString("SampleDownloadId", null, product.SampleDownloadId);
                xmlWriter.WriteElementString("HasUserAgreement", null, product.HasUserAgreement.ToString());
                xmlWriter.WriteElementString("UserAgreementText", null, product.UserAgreementText);
                xmlWriter.WriteElementString("IsRecurring", null, product.IsRecurring.ToString());
                xmlWriter.WriteElementString("RecurringCycleLength", null, product.RecurringCycleLength.ToString());
                xmlWriter.WriteElementString("RecurringCyclePeriodId", null, product.RecurringCyclePeriodId.ToString());
                xmlWriter.WriteElementString("RecurringTotalCycles", null, product.RecurringTotalCycles.ToString());
                xmlWriter.WriteElementString("Interval", null, product.Interval.ToString());
                xmlWriter.WriteElementString("IntervalUnitId", null, product.IntervalUnitId.ToString());
                xmlWriter.WriteElementString("IsShipEnabled", null, product.IsShipEnabled.ToString());
                xmlWriter.WriteElementString("IsFreeShipping", null, product.IsFreeShipping.ToString());
                xmlWriter.WriteElementString("ShipSeparately", null, product.ShipSeparately.ToString());
                xmlWriter.WriteElementString("AdditionalShippingCharge", null, product.AdditionalShippingCharge.ToString());
                xmlWriter.WriteElementString("DeliveryDateId", null, product.DeliveryDateId);
                xmlWriter.WriteElementString("IsTaxExempt", null, product.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("TaxCategoryId", null, product.TaxCategoryId);
                xmlWriter.WriteElementString("IsTele", null, product.IsTele.ToString());
                xmlWriter.WriteElementString("ManageInventoryMethodId", null, product.ManageInventoryMethodId.ToString());
                xmlWriter.WriteElementString("UseMultipleWarehouses", null, product.UseMultipleWarehouses.ToString());
                xmlWriter.WriteElementString("WarehouseId", null, product.WarehouseId);
                xmlWriter.WriteElementString("StockQuantity", null, product.StockQuantity.ToString());
                xmlWriter.WriteElementString("DisplayStockAvailability", null, product.DisplayStockAvailability.ToString());
                xmlWriter.WriteElementString("DisplayStockQuantity", null, product.DisplayStockQuantity.ToString());
                xmlWriter.WriteElementString("MinStockQuantity", null, product.MinStockQuantity.ToString());
                xmlWriter.WriteElementString("LowStockActivityId", null, product.LowStockActivityId.ToString());
                xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, product.NotifyAdminForQuantityBelow.ToString());
                xmlWriter.WriteElementString("BackorderModeId", null, product.BackorderModeId.ToString());
                xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, product.AllowBackInStockSubscriptions.ToString());
                xmlWriter.WriteElementString("OrderMinimumQuantity", null, product.OrderMinimumQuantity.ToString());
                xmlWriter.WriteElementString("OrderMaximumQuantity", null, product.OrderMaximumQuantity.ToString());
                xmlWriter.WriteElementString("AllowedQuantities", null, product.AllowedQuantities);
                xmlWriter.WriteElementString("AllowAddingOnlyExistingAttributeCombinations", null, product.AllowAddingOnlyExistingAttributeCombinations.ToString());
                xmlWriter.WriteElementString("DisableBuyButton", null, product.DisableBuyButton.ToString());
                xmlWriter.WriteElementString("DisableWishlistButton", null, product.DisableWishlistButton.ToString());
                xmlWriter.WriteElementString("AvailableForPreOrder", null, product.AvailableForPreOrder.ToString());
                xmlWriter.WriteElementString("PreOrderAvailabilityStartDateTimeUtc", null, product.PreOrderAvailabilityStartDateTimeUtc.HasValue ? product.PreOrderAvailabilityStartDateTimeUtc.ToString() : "");
                xmlWriter.WriteElementString("CallForPrice", null, product.CallForPrice.ToString());
                xmlWriter.WriteElementString("Price", null, product.Price.ToString());
                xmlWriter.WriteElementString("OldPrice", null, product.OldPrice.ToString());
                xmlWriter.WriteElementString("CatalogPrice", null, product.CatalogPrice.ToString());
                xmlWriter.WriteElementString("ProductCost", null, product.ProductCost.ToString());
                xmlWriter.WriteElementString("CustomerEntersPrice", null, product.CustomerEntersPrice.ToString());
                xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, product.MinimumCustomerEnteredPrice.ToString());
                xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, product.MaximumCustomerEnteredPrice.ToString());
                xmlWriter.WriteElementString("BasepriceEnabled", null, product.BasepriceEnabled.ToString());
                xmlWriter.WriteElementString("BasepriceAmount", null, product.BasepriceAmount.ToString());
                xmlWriter.WriteElementString("BasepriceUnitId", null, product.BasepriceUnitId);
                xmlWriter.WriteElementString("BasepriceBaseAmount", null, product.BasepriceBaseAmount.ToString());
                xmlWriter.WriteElementString("BasepriceBaseUnitId", null, product.BasepriceBaseUnitId);
                xmlWriter.WriteElementString("MarkAsNew", null, product.MarkAsNew.ToString());
                xmlWriter.WriteElementString("MarkAsNewStartDateTimeUtc", null, product.MarkAsNewStartDateTimeUtc.HasValue ? product.MarkAsNewStartDateTimeUtc.ToString() : "");
                xmlWriter.WriteElementString("MarkAsNewEndDateTimeUtc", null, product.MarkAsNewEndDateTimeUtc.HasValue ? product.MarkAsNewEndDateTimeUtc.ToString() : "");
                xmlWriter.WriteElementString("Weight", null, product.Weight.ToString());
                xmlWriter.WriteElementString("Length", null, product.Length.ToString());
                xmlWriter.WriteElementString("Width", null, product.Width.ToString());
                xmlWriter.WriteElementString("Height", null, product.Height.ToString());
                xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("ProductDiscounts");

                foreach (var appliedDiscount in product.AppliedDiscounts)
                {
                    var discount = await _discountService.GetDiscountById(appliedDiscount);
                    if (discount != null)
                    {
                        xmlWriter.WriteStartElement("Discount");
                        xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                        xmlWriter.WriteElementString("Name", null, discount.Name);
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteStartElement("TierPrices");
                var tierPrices = product.TierPrices;
                foreach (var tierPrice in tierPrices)
                {
                    xmlWriter.WriteStartElement("TierPrice");
                    xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id);
                    xmlWriter.WriteElementString("StoreId", null, tierPrice.StoreId);
                    xmlWriter.WriteElementString("CustomerRoleId", null, !String.IsNullOrEmpty(tierPrice.CustomerRoleId) ? tierPrice.CustomerRoleId : "");
                    xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                    xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                    xmlWriter.WriteElementString("StartDateTimeUtc", tierPrice.StartDateTimeUtc.HasValue ? tierPrice.StartDateTimeUtc.Value.ToString() : "");
                    xmlWriter.WriteElementString("EndDateTimeUtc", tierPrice.EndDateTimeUtc.HasValue ? tierPrice.EndDateTimeUtc.Value.ToString() : "");
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductAttributes");
                var productAttributMappings = product.ProductAttributeMappings;
                foreach (var productAttributeMapping in productAttributMappings)
                {
                    var pa = await _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId);
                    xmlWriter.WriteStartElement("ProductAttributeMapping");
                    xmlWriter.WriteElementString("ProductAttributeMappingId", null, productAttributeMapping.Id);
                    xmlWriter.WriteElementString("ProductAttributeId", null, productAttributeMapping.ProductAttributeId);
                    xmlWriter.WriteElementString("ProductAttributeName", null, pa.Name);
                    xmlWriter.WriteElementString("TextPrompt", null, productAttributeMapping.TextPrompt);
                    xmlWriter.WriteElementString("IsRequired", null, productAttributeMapping.IsRequired.ToString());
                    xmlWriter.WriteElementString("AttributeControlTypeId", null, productAttributeMapping.AttributeControlTypeId.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productAttributeMapping.DisplayOrder.ToString());
                    //validation rules
                    if (productAttributeMapping.ValidationRulesAllowed())
                    {
                        if (productAttributeMapping.ValidationMinLength.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationMinLength", null, productAttributeMapping.ValidationMinLength.Value.ToString());
                        }
                        if (productAttributeMapping.ValidationMaxLength.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationMaxLength", null, productAttributeMapping.ValidationMaxLength.Value.ToString());
                        }
                        if (String.IsNullOrEmpty(productAttributeMapping.ValidationFileAllowedExtensions))
                        {
                            xmlWriter.WriteElementString("ValidationFileAllowedExtensions", null, productAttributeMapping.ValidationFileAllowedExtensions);
                        }
                        if (productAttributeMapping.ValidationFileMaximumSize.HasValue)
                        {
                            xmlWriter.WriteElementString("ValidationFileMaximumSize", null, productAttributeMapping.ValidationFileMaximumSize.Value.ToString());
                        }
                        xmlWriter.WriteElementString("DefaultValue", null, productAttributeMapping.DefaultValue);
                    }
                    //conditions
                    xmlWriter.WriteElementString("ConditionAttributeXml", null, productAttributeMapping.ConditionAttributeXml);


                    xmlWriter.WriteStartElement("ProductAttributeValues");
                    var productAttributeValues = productAttributeMapping.ProductAttributeValues;
                    foreach (var productAttributeValue in productAttributeValues)
                    {
                        xmlWriter.WriteStartElement("ProductAttributeValue");
                        xmlWriter.WriteElementString("ProductAttributeValueId", null, productAttributeValue.Id);
                        xmlWriter.WriteElementString("Name", null, productAttributeValue.Name);
                        xmlWriter.WriteElementString("AttributeValueTypeId", null, productAttributeValue.AttributeValueTypeId.ToString());
                        xmlWriter.WriteElementString("AssociatedProductId", null, productAttributeValue.AssociatedProductId);
                        xmlWriter.WriteElementString("ColorSquaresRgb", null, productAttributeValue.ColorSquaresRgb);
                        xmlWriter.WriteElementString("ImageSquaresPictureId", null, productAttributeValue.ImageSquaresPictureId);
                        xmlWriter.WriteElementString("PriceAdjustment", null, productAttributeValue.PriceAdjustment.ToString());
                        xmlWriter.WriteElementString("WeightAdjustment", null, productAttributeValue.WeightAdjustment.ToString());
                        xmlWriter.WriteElementString("Cost", null, productAttributeValue.Cost.ToString());
                        xmlWriter.WriteElementString("Quantity", null, productAttributeValue.Quantity.ToString());
                        xmlWriter.WriteElementString("IsPreSelected", null, productAttributeValue.IsPreSelected.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productAttributeValue.DisplayOrder.ToString());
                        xmlWriter.WriteElementString("PictureId", null, productAttributeValue.PictureId);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();


                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("ProductPictures");
                var productPictures = product.ProductPictures;
                foreach (var productPicture in productPictures)
                {
                    xmlWriter.WriteStartElement("ProductPicture");
                    xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id);
                    xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId);
                    xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductCategories");
                var productCategories = product.ProductCategories;
                if (productCategories != null)
                {
                    foreach (var productCategory in productCategories)
                    {
                        xmlWriter.WriteStartElement("ProductCategory");
                        xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id);
                        xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId);
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductManufacturers");
                var productManufacturers = product.ProductManufacturers;
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        xmlWriter.WriteStartElement("ProductManufacturer");
                        xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id);
                        xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId);
                        xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                var productSpecificationAttributes = product.ProductSpecificationAttributes;
                foreach (var productSpecificationAttribute in productSpecificationAttributes)
                {
                    xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                    xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id);
                    xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId);
                    xmlWriter.WriteElementString("CustomValue", null, productSpecificationAttribute.CustomValue);
                    xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                    xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            await xmlWriter.FlushAsync();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual byte[] ExportProductsToXlsx(IEnumerable<Product> products)
        {

            var properties = new[]
            {
                new PropertyByName<Product>("Id", p => p.Id),
                new PropertyByName<Product>("ProductTypeId", p => p.ProductTypeId),
                new PropertyByName<Product>("ParentGroupedProductId", p => p.ParentGroupedProductId),
                new PropertyByName<Product>("VisibleIndividually", p => p.VisibleIndividually),
                new PropertyByName<Product>("Name", p => p.Name),
                new PropertyByName<Product>("ShortDescription", p => p.ShortDescription),
                new PropertyByName<Product>("FullDescription", p => p.FullDescription),
                new PropertyByName<Product>("Flag", p => p.Flag),
                new PropertyByName<Product>("VendorId", p => p.VendorId),
                new PropertyByName<Product>("ProductTemplateId", p => p.ProductTemplateId),
                new PropertyByName<Product>("ShowOnHomePage", p => p.ShowOnHomePage),
                new PropertyByName<Product>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Product>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Product>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Product>("SeName", p => p.GetSeName("")),
                new PropertyByName<Product>("AllowCustomerReviews", p => p.AllowCustomerReviews),
                new PropertyByName<Product>("Published", p => p.Published),
                new PropertyByName<Product>("SKU", p => p.Sku),
                new PropertyByName<Product>("ManufacturerPartNumber", p => p.ManufacturerPartNumber),
                new PropertyByName<Product>("Gtin", p => p.Gtin),
                new PropertyByName<Product>("IsGiftCard", p => p.IsGiftCard),
                new PropertyByName<Product>("GiftCardTypeId", p => p.GiftCardTypeId),
                new PropertyByName<Product>("OverriddenGiftCardAmount", p => p.OverriddenGiftCardAmount),
                new PropertyByName<Product>("RequireOtherProducts", p => p.RequireOtherProducts),
                new PropertyByName<Product>("RequiredProductIds", p => p.RequiredProductIds),
                new PropertyByName<Product>("AutomaticallyAddRequiredProducts", p => p.AutomaticallyAddRequiredProducts),
                new PropertyByName<Product>("IsDownload", p => p.IsDownload),
                new PropertyByName<Product>("DownloadId", p => p.DownloadId),
                new PropertyByName<Product>("UnlimitedDownloads", p => p.UnlimitedDownloads),
                new PropertyByName<Product>("MaxNumberOfDownloads", p => p.MaxNumberOfDownloads),
                new PropertyByName<Product>("DownloadActivationTypeId", p => p.DownloadActivationTypeId),
                new PropertyByName<Product>("HasSampleDownload", p => p.HasSampleDownload),
                new PropertyByName<Product>("SampleDownloadId", p => p.SampleDownloadId),
                new PropertyByName<Product>("HasUserAgreement", p => p.HasUserAgreement),
                new PropertyByName<Product>("UserAgreementText", p => p.UserAgreementText),
                new PropertyByName<Product>("IsRecurring", p => p.IsRecurring),
                new PropertyByName<Product>("RecurringCycleLength", p => p.RecurringCycleLength),
                new PropertyByName<Product>("RecurringCyclePeriodId", p => p.RecurringCyclePeriodId),
                new PropertyByName<Product>("RecurringTotalCycles", p => p.RecurringTotalCycles),
                new PropertyByName<Product>("Interval", p => p.Interval),
                new PropertyByName<Product>("IntervalUnitId", p => p.IntervalUnitId),
                new PropertyByName<Product>("IsShipEnabled", p => p.IsShipEnabled),
                new PropertyByName<Product>("IsFreeShipping", p => p.IsFreeShipping),
                new PropertyByName<Product>("ShipSeparately", p => p.ShipSeparately),
                new PropertyByName<Product>("AdditionalShippingCharge", p => p.AdditionalShippingCharge),
                new PropertyByName<Product>("DeliveryDateId", p => p.DeliveryDateId),
                new PropertyByName<Product>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<Product>("TaxCategoryId", p => p.TaxCategoryId),
                new PropertyByName<Product>("IsTele", p => p.IsTele),
                new PropertyByName<Product>("ManageInventoryMethodId", p => p.ManageInventoryMethodId),
                new PropertyByName<Product>("UseMultipleWarehouses", p => p.UseMultipleWarehouses),
                new PropertyByName<Product>("WarehouseId", p => p.WarehouseId),
                new PropertyByName<Product>("StockQuantity", p => p.StockQuantity),
                new PropertyByName<Product>("DisplayStockAvailability", p => p.DisplayStockAvailability),
                new PropertyByName<Product>("DisplayStockQuantity", p => p.DisplayStockQuantity),
                new PropertyByName<Product>("MinStockQuantity", p => p.MinStockQuantity),
                new PropertyByName<Product>("LowStockActivityId", p => p.LowStockActivityId),
                new PropertyByName<Product>("NotifyAdminForQuantityBelow", p => p.NotifyAdminForQuantityBelow),
                new PropertyByName<Product>("BackorderModeId", p => p.BackorderModeId),
                new PropertyByName<Product>("AllowBackInStockSubscriptions", p => p.AllowBackInStockSubscriptions),
                new PropertyByName<Product>("OrderMinimumQuantity", p => p.OrderMinimumQuantity),
                new PropertyByName<Product>("OrderMaximumQuantity", p => p.OrderMaximumQuantity),
                new PropertyByName<Product>("AllowedQuantities", p => p.AllowedQuantities),
                new PropertyByName<Product>("AllowAddingOnlyExistingAttributeCombinations", p => p.AllowAddingOnlyExistingAttributeCombinations),
                new PropertyByName<Product>("DisableBuyButton", p => p.DisableBuyButton),
                new PropertyByName<Product>("DisableWishlistButton", p => p.DisableWishlistButton),
                new PropertyByName<Product>("AvailableForPreOrder", p => p.AvailableForPreOrder),
                new PropertyByName<Product>("PreOrderAvailabilityStartDateTimeUtc", p => p.PreOrderAvailabilityStartDateTimeUtc),
                new PropertyByName<Product>("CallForPrice", p => p.CallForPrice),
                new PropertyByName<Product>("Price", p => p.Price),
                new PropertyByName<Product>("OldPrice", p => p.OldPrice),
                new PropertyByName<Product>("CatalogPrice", p => p.CatalogPrice),
                new PropertyByName<Product>("ProductCost", p => p.ProductCost),
                new PropertyByName<Product>("CustomerEntersPrice", p => p.CustomerEntersPrice),
                new PropertyByName<Product>("MinimumCustomerEnteredPrice", p => p.MinimumCustomerEnteredPrice),
                new PropertyByName<Product>("MaximumCustomerEnteredPrice", p => p.MaximumCustomerEnteredPrice),
                new PropertyByName<Product>("BasepriceEnabled", p => p.BasepriceEnabled),
                new PropertyByName<Product>("BasepriceAmount", p => p.BasepriceAmount),
                new PropertyByName<Product>("BasepriceUnitId", p => p.BasepriceUnitId),
                new PropertyByName<Product>("BasepriceBaseAmount", p => p.BasepriceBaseAmount),
                new PropertyByName<Product>("BasepriceBaseUnitId", p => p.BasepriceBaseUnitId),
                new PropertyByName<Product>("MarkAsNew", p => p.MarkAsNew),
                new PropertyByName<Product>("MarkAsNewStartDateTimeUtc", p => p.MarkAsNewStartDateTimeUtc),
                new PropertyByName<Product>("MarkAsNewEndDateTimeUtc", p => p.MarkAsNewEndDateTimeUtc),
                new PropertyByName<Product>("UnitId", p => p.UnitId),
                new PropertyByName<Product>("Weight", p => p.Weight),
                new PropertyByName<Product>("Length", p => p.Length),
                new PropertyByName<Product>("Width", p => p.Width),
                new PropertyByName<Product>("Height", p => p.Height),
                new PropertyByName<Product>("CategoryIds", p =>  string.Join(";", p.ProductCategories.Select(n => n.CategoryId).ToArray())),
                new PropertyByName<Product>("ManufacturerIds", p=>  string.Join(";", p.ProductManufacturers.Select(n => n.ManufacturerId).ToArray())),
                new PropertyByName<Product>("Picture1", p => GetPictures(p).Result[0]),
                new PropertyByName<Product>("Picture2", p => GetPictures(p).Result[1]),
                new PropertyByName<Product>("Picture3", p => GetPictures(p).Result[2])
            };

            return ExportToXlsx(properties, products);
        }

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportOrdersToXml(IList<Order> orders)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true
            };
            var xmlWriter = XmlWriter.Create(stringWriter, xwSettings);
            await xmlWriter.WriteStartDocumentAsync();
            xmlWriter.WriteStartElement("Orders");
            xmlWriter.WriteAttributeString("Version", GrandVersion.FullVersion);


            foreach (var order in orders)
            {
                xmlWriter.WriteStartElement("Order");

                xmlWriter.WriteElementString("OrderId", null, order.Id.ToString());
                xmlWriter.WriteElementString("OrderCode", null, order.Code);
                xmlWriter.WriteElementString("OrderGuid", null, order.OrderGuid.ToString());
                xmlWriter.WriteElementString("StoreId", null, order.StoreId.ToString());
                xmlWriter.WriteElementString("CustomerId", null, order.CustomerId.ToString());
                xmlWriter.WriteElementString("OrderStatusId", null, order.OrderStatusId.ToString());
                xmlWriter.WriteElementString("PaymentStatusId", null, order.PaymentStatusId.ToString());
                xmlWriter.WriteElementString("ShippingStatusId", null, order.ShippingStatusId.ToString());
                xmlWriter.WriteElementString("CustomerLanguageId", null, order.CustomerLanguageId.ToString());
                xmlWriter.WriteElementString("CustomerTaxDisplayTypeId", null, order.CustomerTaxDisplayTypeId.ToString());
                xmlWriter.WriteElementString("CustomerIp", null, order.CustomerIp);
                xmlWriter.WriteElementString("UrlReferrer", null, order.UrlReferrer);
                xmlWriter.WriteElementString("OrderSubtotalInclTax", null, order.OrderSubtotalInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubtotalExclTax", null, order.OrderSubtotalExclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountInclTax", null, order.OrderSubTotalDiscountInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountExclTax", null, order.OrderSubTotalDiscountExclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingInclTax", null, order.OrderShippingInclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingExclTax", null, order.OrderShippingExclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeInclTax", null, order.PaymentMethodAdditionalFeeInclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeExclTax", null, order.PaymentMethodAdditionalFeeExclTax.ToString());
                xmlWriter.WriteElementString("TaxRates", null, order.TaxRates);
                xmlWriter.WriteElementString("OrderTax", null, order.OrderTax.ToString());
                xmlWriter.WriteElementString("OrderTotal", null, order.OrderTotal.ToString());
                xmlWriter.WriteElementString("RefundedAmount", null, order.RefundedAmount.ToString());
                xmlWriter.WriteElementString("OrderDiscount", null, order.OrderDiscount.ToString());
                xmlWriter.WriteElementString("CurrencyRate", null, order.CurrencyRate.ToString());
                xmlWriter.WriteElementString("CustomerCurrencyCode", null, order.CustomerCurrencyCode);
                xmlWriter.WriteElementString("AffiliateId", null, order.AffiliateId);
                xmlWriter.WriteElementString("AllowStoringCreditCardNumber", null, order.AllowStoringCreditCardNumber.ToString());
                xmlWriter.WriteElementString("CardType", null, order.CardType);
                xmlWriter.WriteElementString("CardName", null, order.CardName);
                xmlWriter.WriteElementString("CardNumber", null, order.CardNumber);
                xmlWriter.WriteElementString("MaskedCreditCardNumber", null, order.MaskedCreditCardNumber);
                xmlWriter.WriteElementString("CardCvv2", null, order.CardCvv2);
                xmlWriter.WriteElementString("CardExpirationMonth", null, order.CardExpirationMonth);
                xmlWriter.WriteElementString("CardExpirationYear", null, order.CardExpirationYear);
                xmlWriter.WriteElementString("PaymentMethodSystemName", null, order.PaymentMethodSystemName);
                xmlWriter.WriteElementString("AuthorizationTransactionId", null, order.AuthorizationTransactionId);
                xmlWriter.WriteElementString("AuthorizationTransactionCode", null, order.AuthorizationTransactionCode);
                xmlWriter.WriteElementString("AuthorizationTransactionResult", null, order.AuthorizationTransactionResult);
                xmlWriter.WriteElementString("CaptureTransactionId", null, order.CaptureTransactionId);
                xmlWriter.WriteElementString("CaptureTransactionResult", null, order.CaptureTransactionResult);
                xmlWriter.WriteElementString("SubscriptionTransactionId", null, order.SubscriptionTransactionId);
                xmlWriter.WriteElementString("PaidDateUtc", null, (order.PaidDateUtc == null) ? string.Empty : order.PaidDateUtc.Value.ToString());
                xmlWriter.WriteElementString("ShippingMethod", null, order.ShippingMethod);
                xmlWriter.WriteElementString("ShippingRateComputationMethodSystemName", null, order.ShippingRateComputationMethodSystemName);
                xmlWriter.WriteElementString("CustomValuesXml", null, order.CustomValuesXml);
                xmlWriter.WriteElementString("VatNumber", null, order.VatNumber);
                xmlWriter.WriteElementString("Deleted", null, order.Deleted.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, order.CreatedOnUtc.ToString());

                //products
                var orderItems = order.OrderItems;
                if (orderItems.Count > 0)
                {
                    xmlWriter.WriteStartElement("OrderItems");
                    foreach (var orderItem in orderItems)
                    {
                        xmlWriter.WriteStartElement("OrderItem");
                        xmlWriter.WriteElementString("Id", null, orderItem.Id.ToString());
                        xmlWriter.WriteElementString("OrderItemGuid", null, orderItem.OrderItemGuid.ToString());
                        xmlWriter.WriteElementString("ProductId", null, orderItem.ProductId.ToString());

                        var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                        if (product != null)
                        {
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("UnitPriceInclTax", null, orderItem.UnitPriceInclTax.ToString());
                            xmlWriter.WriteElementString("UnitPriceExclTax", null, orderItem.UnitPriceExclTax.ToString());
                            xmlWriter.WriteElementString("PriceInclTax", null, orderItem.PriceInclTax.ToString());
                            xmlWriter.WriteElementString("PriceExclTax", null, orderItem.PriceExclTax.ToString());
                            xmlWriter.WriteElementString("DiscountAmountInclTax", null, orderItem.DiscountAmountInclTax.ToString());
                            xmlWriter.WriteElementString("DiscountAmountExclTax", null, orderItem.DiscountAmountExclTax.ToString());
                            xmlWriter.WriteElementString("OriginalProductCost", null, orderItem.OriginalProductCost.ToString());
                            xmlWriter.WriteElementString("AttributeDescription", null, orderItem.AttributeDescription);
                            xmlWriter.WriteElementString("AttributesXml", null, orderItem.AttributesXml);
                            xmlWriter.WriteElementString("Quantity", null, orderItem.Quantity.ToString());
                            xmlWriter.WriteElementString("DownloadCount", null, orderItem.DownloadCount.ToString());
                            xmlWriter.WriteElementString("IsDownloadActivated", null, orderItem.IsDownloadActivated.ToString());
                            xmlWriter.WriteElementString("LicenseDownloadId", null, orderItem.LicenseDownloadId.ToString());
                        }
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                //shipments
                var shipmentService = _serviceProvider.GetRequiredService<IShipmentService>();
                var shipments = await shipmentService.GetShipmentsByOrder(order.Id);
                if (shipments.Count > 0)
                {
                    xmlWriter.WriteStartElement("Shipments");
                    foreach (var shipment in shipments)
                    {
                        xmlWriter.WriteStartElement("Shipment");
                        xmlWriter.WriteElementString("ShipmentId", null, shipment.Id.ToString());
                        xmlWriter.WriteElementString("TrackingNumber", null, shipment.TrackingNumber);
                        xmlWriter.WriteElementString("TotalWeight", null, shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value.ToString() : "");

                        xmlWriter.WriteElementString("ShippedDateUtc", null, shipment.ShippedDateUtc.HasValue ?
                            shipment.ShippedDateUtc.ToString() : "");
                        xmlWriter.WriteElementString("DeliveryDateUtc", null, shipment.DeliveryDateUtc.HasValue ?
                            shipment.DeliveryDateUtc.Value.ToString() : "");
                        xmlWriter.WriteElementString("CreatedOnUtc", null, shipment.CreatedOnUtc.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            await xmlWriter.FlushAsync();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        public virtual byte[] ExportOrdersToXlsx(IList<Order> orders)
        {
            return ExportToXlsx(PropertyByOrder(), orders);
        }

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="customers">Customers</param>
        public virtual byte[] ExportCustomersToXlsx(IList<Customer> customers)
        {
            return ExportToXlsx(PropertyByCustomer(), customers);
        }


        /// <summary>
        /// Export customer - personal info to XLSX
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual async Task<byte[]> ExportCustomerToXlsx(Customer customer, string stroreId)
        {
            using (var stream = new MemoryStream())
            {
                IWorkbook xlPackage = new XSSFWorkbook();

                //customer info
                ISheet worksheetCustomer = xlPackage.CreateSheet("CustomerInfo");
                var managerCustomer = PrepareCustomer(customer);
                managerCustomer.WriteToXlsx(worksheetCustomer);

                //address
                var worksheetAddress = xlPackage.CreateSheet("Address");
                var managerAddress = new PropertyManager<Address>(PropertyByAddress());
                managerAddress.WriteCaption(worksheetAddress);
                var row = 1;
                foreach (var item in customer.Addresses)
                {
                    managerAddress.CurrentObject = item;
                    managerAddress.WriteToXlsx(worksheetAddress, row++);
                }

                //orders
                var orderService = _serviceProvider.GetRequiredService<IOrderService>();
                var orders = await orderService.SearchOrders(customerId: customer.Id);
                var worksheetOrder = xlPackage.CreateSheet("Orders");
                var managerOrder = new PropertyManager<Order>(PropertyByOrder());
                managerOrder.WriteCaption(worksheetOrder);
                row = 1;
                foreach (var items in orders)
                {
                    managerOrder.CurrentObject = items;
                    managerOrder.WriteToXlsx(worksheetOrder, row++);
                }

                //activity log
                var customerActivityService = _serviceProvider.GetRequiredService<ICustomerActivityService>();
                var actlogs = await customerActivityService.GetAllActivities(customerId: customer.Id);
                var worksheetLog = xlPackage.CreateSheet("ActivityLogs");
                var managerLog = new PropertyManager<ActivityLog>(PropertyByActivityLog());
                managerLog.WriteCaption(worksheetLog);
                row = 1;
                foreach (var items in actlogs)
                {
                    managerLog.CurrentObject = items;
                    managerLog.WriteToXlsx(worksheetLog, row++);
                }


                //contact us
                var contactUsService = _serviceProvider.GetRequiredService<IContactUsService>();
                var contacts = await contactUsService.GetAllContactUs(customerId: customer.Id);
                var worksheetContact = xlPackage.CreateSheet("MessageContact");
                var managerContact = new PropertyManager<ContactUs>(PropertyByContactForm());
                managerContact.WriteCaption(worksheetContact);
                row = 1;
                foreach (var items in contacts)
                {
                    managerContact.CurrentObject = items;
                    managerContact.WriteToXlsx(worksheetContact, row++);
                }

                //emails
                var queuedEmailService = _serviceProvider.GetRequiredService<IQueuedEmailService>();
                var queuedEmails = await queuedEmailService.SearchEmails("", customer.Email, null, null, false, true, 100, true);
                var worksheetEmails = xlPackage.CreateSheet("Emails");
                var managerEmails = new PropertyManager<QueuedEmail>(PropertyByEmails());
                managerEmails.WriteCaption(worksheetEmails);
                row = 1;
                foreach (var items in queuedEmails)
                {
                    managerEmails.CurrentObject = items;
                    managerEmails.WriteToXlsx(worksheetEmails, row++);
                }

                //Newsletter subscribe - history of change
                var newsletterService = _serviceProvider.GetRequiredService<INewsLetterSubscriptionService>();
                var newsletter = await newsletterService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, stroreId);
                if (newsletter != null)
                {
                    var worksheetNewsletter = xlPackage.CreateSheet("Newsletter subscribe - history of change");
                    var managerNewsletter = new PropertyManager<NewsLetterSubscription>(PropertyByNewsLetterSubscription());
                    var newsletterhistory = await newsletter.GetHistoryObject(_serviceProvider.GetRequiredService<IHistoryService>());
                    managerNewsletter.WriteCaption(worksheetNewsletter);
                    row = 1;
                    foreach (var item in newsletterhistory)
                    {
                        var _tmp = (NewsLetterSubscription)item.Object;

                        var newslettertml = new NewsLetterSubscription() {
                            Active = _tmp.Active,
                            CreatedOnUtc = item.CreatedOnUtc
                        };
                        _tmp.Categories.ToList().ForEach(x => newslettertml.Categories.Add(x));
                        managerNewsletter.CurrentObject = newslettertml;
                        managerNewsletter.WriteToXlsx(worksheetNewsletter, row++);
                    }
                }

                xlPackage.Write(stream);
                return stream.ToArray();
            }

        }


        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        public virtual async Task<string> ExportCustomersToXml(IList<Customer> customers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true,
            };
            var xmlWriter = XmlWriter.Create(stringWriter, xwSettings);
            await xmlWriter.WriteStartDocumentAsync();
            xmlWriter.WriteStartElement("Customers");
            xmlWriter.WriteAttributeString("Version", GrandVersion.FullVersion);

            foreach (var customer in customers)
            {
                xmlWriter.WriteStartElement("Customer");
                xmlWriter.WriteElementString("CustomerId", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);
                xmlWriter.WriteElementString("Password", null, customer.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, customer.PasswordFormatId.ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customer.PasswordSalt);
                xmlWriter.WriteElementString("IsTaxExempt", null, customer.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("AffiliateId", null, customer.AffiliateId);
                xmlWriter.WriteElementString("VendorId", null, customer.VendorId);
                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());


                xmlWriter.WriteElementString("IsGuest", null, customer.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, customer.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, customer.IsAdmin().ToString());
                xmlWriter.WriteElementString("IsForumModerator", null, customer.IsForumModerator().ToString());

                xmlWriter.WriteElementString("FirstName", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId));
                xmlWriter.WriteElementString("StateProvinceId", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId));
                xmlWriter.WriteElementString("StreetAddress", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City));
                xmlWriter.WriteElementString("Phone", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax));
                xmlWriter.WriteElementString("VatNumber", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber));
                xmlWriter.WriteElementString("VatNumberStatusId", null, customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId).ToString());
                xmlWriter.WriteElementString("TimeZoneId", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.TimeZoneId));

                foreach (var store in await _storeService.GetAllStores())
                {
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    bool subscribedToNewsletters = newsletter != null && newsletter.Active;
                    xmlWriter.WriteElementString(string.Format("Newsletter-in-store-{0}", store.Id), null, subscribedToNewsletters.ToString());
                }

                xmlWriter.WriteElementString("AvatarPictureId", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId)?.ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature));

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            await xmlWriter.FlushAsync();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions)
        {
            if (subscriptions == null)
                throw new ArgumentNullException("subscriptions");

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var subscription in subscriptions)
            {
                sb.Append(subscription.Email);
                sb.Append(separator);
                sb.Append(subscription.Active);
                sb.Append(separator);
                sb.Append(subscription.CreatedOnUtc);
                sb.Append(separator);
                sb.Append(subscription.StoreId);
                sb.Append(separator);
                sb.Append(string.Join(';', subscription.Categories));
                sb.Append(Environment.NewLine);  //new line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportNewsletterSubscribersToTxt(IList<string> subscriptions)
        {
            if (subscriptions == null)
                throw new ArgumentNullException("subscriptions");

            var sb = new StringBuilder();
            foreach (var subscription in subscriptions)
            {
                sb.Append(subscription);
                sb.Append(Environment.NewLine);  //new line
            }
            return sb.ToString();
        }
        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual async Task<string> ExportStatesToTxt(IList<StateProvince> states)
        {
            if (states == null)
                throw new ArgumentNullException("states");

            const string separator = ",";
            var sb = new StringBuilder();
            var countryService = _serviceProvider.GetRequiredService<ICountryService>();
            foreach (var state in states)
            {
                var country = await countryService.GetCountryById(state.CountryId);
                sb.Append(country.TwoLetterIsoCode);
                sb.Append(separator);
                sb.Append(state.Name);
                sb.Append(separator);
                sb.Append(state.Abbreviation);
                sb.Append(separator);
                sb.Append(state.Published);
                sb.Append(separator);
                sb.Append(state.DisplayOrder);
                sb.Append(Environment.NewLine);  //new line
            }
            return sb.ToString();
        }


        /// <summary>
        /// Export objects to XLSX
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="properties">Class access to the object through its properties</param>
        /// <param name="itemsToExport">The objects to export</param>
        /// <returns></returns>
        public virtual byte[] ExportToXlsx<T>(PropertyByName<T>[] properties, IEnumerable<T> itemsToExport)
        {
            using (var stream = new MemoryStream())
            {
                IWorkbook xlPackage = new XSSFWorkbook();
                ISheet worksheet = xlPackage.CreateSheet(typeof(T).Name);
                var manager = new PropertyManager<T>(properties);
                manager.WriteCaption(worksheet);

                var row = 1;

                foreach (var items in itemsToExport)
                {
                    manager.CurrentObject = items;
                    manager.WriteToXlsx(worksheet, row++);
                }
                xlPackage.Write(stream);
                return stream.ToArray();
            }
        }

        private async Task<string[]> GetPictures(Product product)
        {
            string picture1 = null;
            string picture2 = null;
            string picture3 = null;
            int i = 0;
            foreach (var picture in product.ProductPictures.Take(3))
            {
                var pic = await _pictureService.GetPictureById(picture.PictureId);
                var pictureLocalPath = await _pictureService.GetThumbLocalPath(pic);
                switch (i)
                {
                    case 0:
                        picture1 = pictureLocalPath;
                        break;
                    case 1:
                        picture2 = pictureLocalPath;
                        break;
                    case 2:
                        picture3 = pictureLocalPath;
                        break;
                }
                i++;
            }

            return new[] { picture1, picture2, picture3 };
        }

        /// <summary>
        /// Returns the path to the image file by ID
        /// </summary>
        /// <param name="pictureId">Picture ID</param>
        /// <returns>Path to the image file</returns>
        protected virtual async Task<string> GetPictures(string pictureId)
        {
            var picture = await _pictureService.GetPictureById(pictureId);
            return await _pictureService.GetThumbLocalPath(picture);
        }


        private PropertyByName<Order>[] PropertyByOrder()
        {
            var properties = new[]
            {
                    new PropertyByName<Order>("OrderNumber", p=>p.OrderNumber),
                    new PropertyByName<Order>("OrderCode", p=>p.Code),
                    new PropertyByName<Order>("OrderId", p=>p.Id),
                    new PropertyByName<Order>("StoreId", p=>p.StoreId),
                    new PropertyByName<Order>("OrderGuid",p=>p.OrderGuid),
                    new PropertyByName<Order>("CustomerId",p=>p.CustomerId),
                    new PropertyByName<Order>("OrderStatusId", p=>p.OrderStatusId),
                    new PropertyByName<Order>("PaymentStatusId", p=>p.PaymentStatusId),
                    new PropertyByName<Order>("ShippingStatusId", p=>p.ShippingStatusId),
                    new PropertyByName<Order>("OrderSubtotalInclTax", p=>p.OrderSubtotalInclTax),
                    new PropertyByName<Order>("OrderSubtotalExclTax", p=>p.OrderSubtotalExclTax),
                    new PropertyByName<Order>("OrderSubTotalDiscountInclTax", p=>p.OrderSubTotalDiscountInclTax),
                    new PropertyByName<Order>("OrderSubTotalDiscountExclTax", p=>p.OrderSubTotalDiscountExclTax),
                    new PropertyByName<Order>("OrderShippingInclTax", p=>p.OrderShippingInclTax),
                    new PropertyByName<Order>("OrderShippingExclTax", p=>p.OrderShippingExclTax),
                    new PropertyByName<Order>("PaymentMethodAdditionalFeeInclTax", p=>p.PaymentMethodAdditionalFeeInclTax),
                    new PropertyByName<Order>("PaymentMethodAdditionalFeeExclTax", p=>p.PaymentMethodAdditionalFeeExclTax),
                    new PropertyByName<Order>("TaxRates", p=>p.TaxRates),
                    new PropertyByName<Order>("OrderTax", p=>p.OrderTax),
                    new PropertyByName<Order>("OrderTotal", p=>p.OrderTotal),
                    new PropertyByName<Order>("RefundedAmount", p=>p.RefundedAmount),
                    new PropertyByName<Order>("OrderDiscount", p=>p.OrderDiscount),
                    new PropertyByName<Order>("CurrencyRate", p=>p.CurrencyRate),
                    new PropertyByName<Order>("CustomerCurrencyCode", p=>p.CustomerCurrencyCode),
                    new PropertyByName<Order>("AffiliateId", p=>p.AffiliateId),
                    new PropertyByName<Order>("PaymentMethodSystemName", p=>p.PaymentMethodSystemName),
                    new PropertyByName<Order>("ShippingPickUpInStore", p=>p.PickUpInStore),
                    new PropertyByName<Order>("ShippingMethod", p=>p.ShippingMethod),
                    new PropertyByName<Order>("ShippingRateComputationMethodSystemName", p=>p.ShippingRateComputationMethodSystemName),
                    new PropertyByName<Order>("CustomValuesXml", p=>p.CustomValuesXml),
                    new PropertyByName<Order>("VatNumber", p=>p.VatNumber),
                    new PropertyByName<Order>("CreatedOnUtc", p=>p.CreatedOnUtc.ToOADate()),
                    new PropertyByName<Order>("BillingFirstName", p=>p.BillingAddress.Return(billingAddress=>billingAddress.FirstName, "")),
                    new PropertyByName<Order>("BillingLastName", p=>p.BillingAddress.Return(billingAddress=>billingAddress.LastName, "")),
                    new PropertyByName<Order>("BillingEmail", p=>p.BillingAddress.Return(billingAddress=>billingAddress.Email, "")),
                    new PropertyByName<Order>("BillingCompany", p=>p.BillingAddress.Return(billingAddress=>billingAddress.Company, "")),
                    new PropertyByName<Order>("BillingVatNumber", p=>p.BillingAddress.Return(billingAddress=>billingAddress.VatNumber, "")),
                    new PropertyByName<Order>("BillingCountry",p=>p.BillingAddress.Return(billingAddress=>_serviceProvider.GetRequiredService<ICountryService>().GetCountryById(billingAddress.CountryId).Result, null).Return(country=>country.Name,"")),
                    new PropertyByName<Order>("BillingStateProvince",p=>p.BillingAddress.Return(billingAddress=>_serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(billingAddress.StateProvinceId).Result, null).Return(stateProvince=>stateProvince.Name,"")),
                    new PropertyByName<Order>("BillingCity", p=>p.BillingAddress.Return(billingAddress=>billingAddress.City,"")),
                    new PropertyByName<Order>("BillingAddress1",p=>p.BillingAddress.Return(billingAddress=>billingAddress.Address1,"")),
                    new PropertyByName<Order>("BillingAddress2", p=>p.BillingAddress.Return(billingAddress=>billingAddress.Address2,"")),
                    new PropertyByName<Order>("BillingZipPostalCode", p=>p.BillingAddress.Return(billingAddress=>billingAddress.ZipPostalCode,"")),
                    new PropertyByName<Order>("BillingPhoneNumber", p=>p.BillingAddress.Return(billingAddress=>billingAddress.PhoneNumber,"")),
                    new PropertyByName<Order>("BillingFaxNumber", p=>p.BillingAddress.Return(billingAddress=>billingAddress.FaxNumber,"")),
                    new PropertyByName<Order>("ShippingFirstName", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.FirstName,"")),
                    new PropertyByName<Order>("ShippingLastName", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.LastName, "")),
                    new PropertyByName<Order>("ShippingEmail", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.Email, "")),
                    new PropertyByName<Order>("ShippingCompany", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.Company, "")),
                    new PropertyByName<Order>("ShippingVatNumber", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.VatNumber, "")),
                    new PropertyByName<Order>("ShippingCountry", p=>p.ShippingAddress.Return(shippingAddress=>_serviceProvider.GetRequiredService<ICountryService>().GetCountryById(shippingAddress.CountryId).Result, null).Return(country=>country.Name,"")),
                    new PropertyByName<Order>("ShippingStateProvince", p=>p.ShippingAddress.Return(shippingAddress=>_serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(shippingAddress.StateProvinceId).Result, null).Return(stateProvince=>stateProvince.Name,"")),
                    new PropertyByName<Order>("ShippingCity", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.City, "")),
                    new PropertyByName<Order>("ShippingAddress1", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.Address1, "")),
                    new PropertyByName<Order>("ShippingAddress2", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.Address2, "")),
                    new PropertyByName<Order>("ShippingZipPostalCode", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.ZipPostalCode, "")),
                    new PropertyByName<Order>("ShippingPhoneNumber",p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.PhoneNumber, "")),
                    new PropertyByName<Order>("ShippingFaxNumber", p=>p.ShippingAddress.Return(shippingAddress=>shippingAddress.FaxNumber, ""))
            };
            return properties;
        }

        private PropertyByName<Address>[] PropertyByAddress()
        {
            var properties = new[]
            {
                    new PropertyByName<Address>("Email", p=>p.Email),
                    new PropertyByName<Address>("FirstName", p=>p.FirstName),
                    new PropertyByName<Address>("LastName", p=>p.LastName),
                    new PropertyByName<Address>("PhoneNumber", p=>p.PhoneNumber),
                    new PropertyByName<Address>("FaxNumber", p=>p.FaxNumber),
                    new PropertyByName<Address>("Address1", p=>p.Address1),
                    new PropertyByName<Address>("Address2", p=>p.Address2),
                    new PropertyByName<Address>("City", p=>p.City),
                    new PropertyByName<Address>("Country", p=> !string.IsNullOrEmpty(p.CountryId) ? _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(p.CountryId).Result?.Name : ""),
                    new PropertyByName<Address>("StateProvince", p=> !string.IsNullOrEmpty(p.StateProvinceId) ? _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(p.StateProvinceId).Result?.Name : ""),
            };
            return properties;
        }

        private PropertyByName<Customer>[] PropertyByCustomer()
        {
            var properties = new[]
            {
                new PropertyByName<Customer>("CustomerId", p => p.Id),
                new PropertyByName<Customer>("CustomerGuid", p => p.CustomerGuid),
                new PropertyByName<Customer>("Email", p => p.Email),
                new PropertyByName<Customer>("Username", p => p.Username),
                new PropertyByName<Customer>("Password", p => p.Password),
                new PropertyByName<Customer>("PasswordFormatId", p => p.PasswordFormatId),
                new PropertyByName<Customer>("PasswordSalt", p => p.PasswordSalt),
                new PropertyByName<Customer>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<Customer>("AffiliateId", p => p.AffiliateId),
                new PropertyByName<Customer>("VendorId", p => p.VendorId),
                new PropertyByName<Customer>("Active", p => p.Active),
                new PropertyByName<Customer>("IsGuest", p => p.IsGuest()),
                new PropertyByName<Customer>("IsRegistered", p => p.IsRegistered()),
                new PropertyByName<Customer>("IsAdministrator", p => p.IsAdmin()),
                new PropertyByName<Customer>("IsForumModerator", p => p.IsForumModerator()),
                //attributes
                new PropertyByName<Customer>("FirstName", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName)),
                new PropertyByName<Customer>("LastName", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName)),
                new PropertyByName<Customer>("Gender", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender)),
                new PropertyByName<Customer>("Company", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company)),
                new PropertyByName<Customer>("StreetAddress", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress)),
                new PropertyByName<Customer>("StreetAddress2", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2)),
                new PropertyByName<Customer>("ZipPostalCode", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode)),
                new PropertyByName<Customer>("City", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City)),
                new PropertyByName<Customer>("CountryId", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId)),
                new PropertyByName<Customer>("StateProvinceId", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId)),
                new PropertyByName<Customer>("Phone", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone)),
                new PropertyByName<Customer>("Fax", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax)),
                new PropertyByName<Customer>("VatNumber", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber)),
                new PropertyByName<Customer>("VatNumberStatusId", p => p.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId)),
                new PropertyByName<Customer>("TimeZoneId", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.TimeZoneId)),
                new PropertyByName<Customer>("AvatarPictureId", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId)),
                new PropertyByName<Customer>("ForumPostCount", p => p.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount)),
                new PropertyByName<Customer>("Signature", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature)),
            };
            return properties;
        }

        private PropertyByName<ActivityLog>[] PropertyByActivityLog()
        {
            var properties = new[]
            {
                new PropertyByName<ActivityLog>("IpAddress", p => p.IpAddress),
                new PropertyByName<ActivityLog>("CreatedOnUtc", p => p.CreatedOnUtc.ToString()),
                new PropertyByName<ActivityLog>("Comment", p => p.Comment),
            };
            return properties;
        }
        private PropertyByName<ContactUs>[] PropertyByContactForm()
        {
            var properties = new[]
            {
                new PropertyByName<ContactUs>("IpAddress", p => p.IpAddress),
                new PropertyByName<ContactUs>("CreatedOnUtc", p => p.CreatedOnUtc.ToString()),
                new PropertyByName<ContactUs>("Email", p => p.Email),
                new PropertyByName<ContactUs>("FullName", p => p.FullName),
                new PropertyByName<ContactUs>("Subject", p => p.Subject),
                new PropertyByName<ContactUs>("Enquiry", p => p.Enquiry),
                new PropertyByName<ContactUs>("ContactAttributeDescription", p => p.ContactAttributeDescription),
            };
            return properties;
        }

        private PropertyByName<QueuedEmail>[] PropertyByEmails()
        {
            var properties = new[]
            {
                new PropertyByName<QueuedEmail>("SentOnUtc", p => p.SentOnUtc.ToString()),
                new PropertyByName<QueuedEmail>("From", p => p.From),
                new PropertyByName<QueuedEmail>("FromName", p => p.FromName),
                new PropertyByName<QueuedEmail>("Subject", p => p.Subject),
                new PropertyByName<QueuedEmail>("Body", p => p.Body),
            };
            return properties;
        }

        private PropertyByName<NewsLetterSubscription>[] PropertyByNewsLetterSubscription()
        {
            var newsletterCategoryService = _serviceProvider.GetRequiredService<INewsletterCategoryService>();

            string GetCategoryNames(IList<string> categoryNames, string separator = ",")
            {
                var sb = new StringBuilder();
                for (int i = 0; i < categoryNames.Count; i++)
                {
                    var category = newsletterCategoryService.GetNewsletterCategoryById(categoryNames[i]).Result;
                    if (category != null)
                    {
                        sb.Append(category.Name);
                        if (i != categoryNames.Count - 1)
                        {
                            sb.Append(separator);
                            sb.Append(" ");
                        }
                    }
                }
                return sb.ToString();
            }
            var properties = new[]
            {
                new PropertyByName<NewsLetterSubscription>("CreatedOnUtc", p => p.CreatedOnUtc.ToString()),
                new PropertyByName<NewsLetterSubscription>("Active", p => p.Active.ToString()),
                new PropertyByName<NewsLetterSubscription>("Categories", p => GetCategoryNames(p.Categories.ToList())),

            };
            return properties;
        }

        private PropertyHelperList<Customer> PrepareCustomer(Customer customer)
        {
            var helper = new PropertyHelperList<Customer>(customer);
            helper.ObjectList.Add(new PropertyHelperList<Customer>("CustomerId", p => p.Id));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("CustomerGuid", p => p.CustomerGuid));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Email", p => p.Email));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Username", p => p.Username));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Password", p => p.Password));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("PasswordFormatId", p => p.PasswordFormatId));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("PasswordSalt", p => p.PasswordSalt));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("IsTaxExempt", p => p.IsTaxExempt));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Active", p => p.Active));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("IsGuest", p => p.IsGuest()));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("IsRegistered", p => p.IsRegistered()));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("IsAdministrator", p => p.IsAdmin()));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("IsForumModerator", p => p.IsForumModerator()));
            //attributes
            helper.ObjectList.Add(new PropertyHelperList<Customer>("FirstName", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("LastName", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Gender", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Company", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("StreetAddress", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("StreetAddress2", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("ZipPostalCode", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("City", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Country",
                p =>
                {
                    var countryid = p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                    var countryName = "";
                    if (!string.IsNullOrEmpty(countryid))
                        countryName = _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(countryid).Result?.Name;
                    return countryName;
                }
                ));

            helper.ObjectList.Add(new PropertyHelperList<Customer>("StateProvince",
                p =>
                {
                    var stateId = p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StateProvinceId);
                    var stateName = "";
                    if (!string.IsNullOrEmpty(stateId))
                        stateName = _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(stateId).Result?.Name;
                    return stateName;
                }
                ));

            helper.ObjectList.Add(new PropertyHelperList<Customer>("Phone", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Fax", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("VatNumber", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("ForumPostCount", p => p.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount)));
            helper.ObjectList.Add(new PropertyHelperList<Customer>("Signature", p => p.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature)));

            return helper;
        }

        #endregion
    }
}
