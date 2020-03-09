using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Seo;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductService : ICopyProductService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly SeoSettings _seoSettings;
        #endregion

        #region Ctor

        public CopyProductService(IProductService productService,
            ILanguageService languageService,
            IPictureService pictureService,
            IDownloadService downloadService,
            IUrlRecordService urlRecordService,
            SeoSettings seoSettings)
        {
            _productService = productService;
            _languageService = languageService;
            _pictureService = pictureService;
            _downloadService = downloadService;
            _urlRecordService = urlRecordService;
            _seoSettings = seoSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a copy of product with all depended data
        /// </summary>
        /// <param name="product">The product to copy</param>
        /// <param name="newName">The name of product duplicate</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
        /// <param name="copyImages">A value indicating whether the product images should be copied</param>
        /// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
        /// <returns>Product copy</returns>
        public virtual async Task<Product> CopyProduct(Product product, string newName,
            bool isPublished = true, bool copyImages = true, bool copyAssociatedProducts = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (String.IsNullOrEmpty(newName))
                throw new ArgumentException("Product name is required");

            //product download & sample download
            string downloadId = product.DownloadId;
            string sampleDownloadId = product.SampleDownloadId;
            if (product.IsDownload)
            {
                var download = await _downloadService.GetDownloadById(product.DownloadId);
                if (download != null)
                {
                    var downloadCopy = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = download.UseDownloadUrl,
                        DownloadUrl = download.DownloadUrl,
                        DownloadBinary = download.DownloadBinary,
                        ContentType = download.ContentType,
                        Filename = download.Filename,
                        Extension = download.Extension,
                        IsNew = download.IsNew,
                    };
                    await _downloadService.InsertDownload(downloadCopy);
                    downloadId = downloadCopy.Id;
                }

                if (product.HasSampleDownload)
                {
                    var sampleDownload = await _downloadService.GetDownloadById(product.SampleDownloadId);
                    if (sampleDownload != null)
                    {
                        var sampleDownloadCopy = new Download
                        {
                            DownloadGuid = Guid.NewGuid(),
                            UseDownloadUrl = sampleDownload.UseDownloadUrl,
                            DownloadUrl = sampleDownload.DownloadUrl,
                            DownloadBinary = sampleDownload.DownloadBinary,
                            ContentType = sampleDownload.ContentType,
                            Filename = sampleDownload.Filename,
                            Extension = sampleDownload.Extension,
                            IsNew = sampleDownload.IsNew
                        };
                        await _downloadService.InsertDownload(sampleDownloadCopy);
                        sampleDownloadId = sampleDownloadCopy.Id;
                    }
                }
            }

            // product
            var productCopy = new Product
            {
                ProductTypeId = product.ProductTypeId,
                ParentGroupedProductId = product.ParentGroupedProductId,
                VisibleIndividually = product.VisibleIndividually,
                Name = newName,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                Flag = product.Flag,
                VendorId = product.VendorId,
                ProductTemplateId = product.ProductTemplateId,
                AdminComment = product.AdminComment,
                ShowOnHomePage = product.ShowOnHomePage,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                MetaTitle = product.MetaTitle,
                AllowCustomerReviews = product.AllowCustomerReviews,
                LimitedToStores = product.LimitedToStores,
                Sku = product.Sku,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                Gtin = product.Gtin,
                IsGiftCard = product.IsGiftCard,
                GiftCardType = product.GiftCardType,
                OverriddenGiftCardAmount = product.OverriddenGiftCardAmount,
                RequireOtherProducts = product.RequireOtherProducts,
                RequiredProductIds = product.RequiredProductIds,
                AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts,
                IsDownload = product.IsDownload,
                DownloadId = downloadId,
                UnlimitedDownloads = product.UnlimitedDownloads,
                MaxNumberOfDownloads = product.MaxNumberOfDownloads,
                DownloadExpirationDays = product.DownloadExpirationDays,
                DownloadActivationType = product.DownloadActivationType,
                HasSampleDownload = product.HasSampleDownload,
                SampleDownloadId = sampleDownloadId,
                HasUserAgreement = product.HasUserAgreement,
                UserAgreementText = product.UserAgreementText,
                IsRecurring = product.IsRecurring,
                RecurringCycleLength = product.RecurringCycleLength,
                RecurringCyclePeriod = product.RecurringCyclePeriod,
                RecurringTotalCycles = product.RecurringTotalCycles,
                IsShipEnabled = product.IsShipEnabled,
                IsFreeShipping = product.IsFreeShipping,
                ShipSeparately = product.ShipSeparately,
                AdditionalShippingCharge = product.AdditionalShippingCharge,
                DeliveryDateId = product.DeliveryDateId,
                IsTaxExempt = product.IsTaxExempt,
                TaxCategoryId = product.TaxCategoryId,
                IsTele = product.IsTele,
                ManageInventoryMethod = product.ManageInventoryMethod,
                UseMultipleWarehouses = product.UseMultipleWarehouses,
                WarehouseId = product.WarehouseId,
                StockQuantity = product.StockQuantity,
                DisplayStockAvailability = product.DisplayStockAvailability,
                DisplayStockQuantity = product.DisplayStockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                LowStock = product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity,
                LowStockActivityId = product.LowStockActivityId,
                NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow,
                BackorderMode = product.BackorderMode,
                AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions,
                OrderMinimumQuantity = product.OrderMinimumQuantity,
                OrderMaximumQuantity = product.OrderMaximumQuantity,
                AllowedQuantities = product.AllowedQuantities,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                DisableBuyButton = product.DisableBuyButton,
                DisableWishlistButton = product.DisableWishlistButton,
                AvailableForPreOrder = product.AvailableForPreOrder,
                PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc,
                CallForPrice = product.CallForPrice,
                Price = product.Price,
                OldPrice = product.OldPrice,
                CatalogPrice = product.CatalogPrice,
                StartPrice = product.StartPrice,
                ProductCost = product.ProductCost,
                CustomerEntersPrice = product.CustomerEntersPrice,
                MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice,
                MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice,
                BasepriceEnabled = product.BasepriceEnabled,
                BasepriceAmount = product.BasepriceAmount,
                BasepriceUnitId = product.BasepriceUnitId,
                BasepriceBaseAmount = product.BasepriceBaseAmount,
                BasepriceBaseUnitId = product.BasepriceBaseUnitId,
                MarkAsNew = product.MarkAsNew,
                MarkAsNewStartDateTimeUtc = product.MarkAsNewStartDateTimeUtc,
                MarkAsNewEndDateTimeUtc = product.MarkAsNewEndDateTimeUtc,
                Weight = product.Weight,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                DisplayOrder = product.DisplayOrder,
                Published = isPublished,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                Locales = product.Locales,
                CustomerRoles = product.CustomerRoles,
                Stores = product.Stores
            };

            // product <-> warehouses mappings
            foreach (var pwi in product.ProductWarehouseInventory)
            {
                productCopy.ProductWarehouseInventory.Add(pwi);
            }

            // product <-> categories mappings
            foreach (var productCategory in product.ProductCategories)
            {
                productCopy.ProductCategories.Add(productCategory);
            }

            // product <-> manufacturers mappings
            foreach (var productManufacturers in product.ProductManufacturers)
            {
                productCopy.ProductManufacturers.Add(productManufacturers);
            }

            // product <-> releated products mappings
            foreach (var relatedProduct in product.RelatedProducts)
            {
                productCopy.RelatedProducts.Add(relatedProduct);
            }

            //product tags
            foreach (var productTag in product.ProductTags)
            {
                productCopy.ProductTags.Add(productTag);
            }

            // product <-> attributes mappings
            foreach (var productAttributeMapping in product.ProductAttributeMappings)
            {
                productCopy.ProductAttributeMappings.Add(productAttributeMapping);
            }
            //attribute combinations
            foreach (var combination in product.ProductAttributeCombinations)
            {
                productCopy.ProductAttributeCombinations.Add(combination);
            }

            foreach (var csProduct in product.CrossSellProduct)
            {
                productCopy.CrossSellProduct.Add(csProduct);
            }

            // product specifications
            foreach (var productSpecificationAttribute in product.ProductSpecificationAttributes)
            {
                productCopy.ProductSpecificationAttributes.Add(productSpecificationAttribute);
            }

            //tier prices
            foreach (var tierPrice in product.TierPrices)
            {
                productCopy.TierPrices.Add(tierPrice);
            }

            // product <-> discounts mapping
            foreach (var discount in product.AppliedDiscounts)
            {
                productCopy.AppliedDiscounts.Add(discount);
                
            }

            //validate search engine name
            await _productService.InsertProduct(productCopy);

            //search engine name
            string seName = await productCopy.ValidateSeName("", productCopy.Name, true, _seoSettings, _urlRecordService, _languageService);
            productCopy.SeName = seName;
            await _productService.UpdateProduct(productCopy);
            await _urlRecordService.SaveSlug(productCopy, seName, "");

            var languages = await _languageService.GetAllLanguages(true);

            //product pictures
            //variable to store original and new picture identifiers
            int id = 1;
            var originalNewPictureIdentifiers = new Dictionary<string, string>();
            if (copyImages)
            {
                foreach (var productPicture in product.ProductPictures)
                {
                    var picture = await _pictureService.GetPictureById(productPicture.PictureId);
                    var pictureCopy = await _pictureService.InsertPicture(
                        await _pictureService.LoadPictureBinary(picture),
                        picture.MimeType,
                        _pictureService.GetPictureSeName(newName),
                        picture.AltAttribute,
                        picture.TitleAttribute);

                    await _productService.InsertProductPicture(new ProductPicture
                    {
                        ProductId = productCopy.Id,
                        PictureId = pictureCopy.Id,
                        DisplayOrder = productPicture.DisplayOrder
                    });
                    id++;
                    originalNewPictureIdentifiers.Add(picture.Id, pictureCopy.Id);
                }
            }


            return productCopy;
        }

        #endregion
    }
}