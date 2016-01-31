using System;
using System.Linq;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using MongoDB.Bson;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductService : ICopyProductService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public CopyProductService(IProductService productService,
            IProductAttributeService productAttributeService,
            ILanguageService languageService,
            IPictureService pictureService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
            IDownloadService downloadService,
            IProductAttributeParser productAttributeParser,
            IUrlRecordService urlRecordService,
            IStoreMappingService storeMappingService)
        {
            this._productService = productService;
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._pictureService = pictureService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._specificationAttributeService = specificationAttributeService;
            this._downloadService = downloadService;
            this._productAttributeParser = productAttributeParser;
            this._urlRecordService = urlRecordService;
            this._storeMappingService = storeMappingService;
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
        public virtual Product CopyProduct(Product product, string newName,
            bool isPublished = true, bool copyImages = true, bool copyAssociatedProducts = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (String.IsNullOrEmpty(newName))
                throw new ArgumentException("Product name is required");

            //product download & sample download
            int downloadId = product.DownloadId;
            int sampleDownloadId = product.SampleDownloadId;
            if (product.IsDownload)
            {
                var download = _downloadService.GetDownloadById(product.DownloadId);
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
                    _downloadService.InsertDownload(downloadCopy);
                    downloadId = downloadCopy.Id;
                }

                if (product.HasSampleDownload)
                {
                    var sampleDownload = _downloadService.GetDownloadById(product.SampleDownloadId);
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
                        _downloadService.InsertDownload(sampleDownloadCopy);
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
                IsRental = product.IsRental,
                RentalPriceLength = product.RentalPriceLength,
                RentalPricePeriod = product.RentalPricePeriod,
                IsShipEnabled = product.IsShipEnabled,
                IsFreeShipping = product.IsFreeShipping,
                ShipSeparately = product.ShipSeparately,
                AdditionalShippingCharge = product.AdditionalShippingCharge,
                DeliveryDateId = product.DeliveryDateId,
                IsTaxExempt = product.IsTaxExempt,
                TaxCategoryId = product.TaxCategoryId,
                IsTelecommunicationsOrBroadcastingOrElectronicServices = product.IsTelecommunicationsOrBroadcastingOrElectronicServices,
                ManageInventoryMethod = product.ManageInventoryMethod,
                UseMultipleWarehouses = product.UseMultipleWarehouses,
                WarehouseId = product.WarehouseId,
                StockQuantity = product.StockQuantity,
                DisplayStockAvailability = product.DisplayStockAvailability,
                DisplayStockQuantity = product.DisplayStockQuantity,
                MinStockQuantity = product.MinStockQuantity,
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
                ProductCost = product.ProductCost,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc,
                SpecialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc,
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

            //validate search engine name
            _productService.InsertProduct(productCopy);

            //search engine name
            string seName = productCopy.ValidateSeName("", productCopy.Name, true);
            productCopy.SeName = seName;
            _productService.UpdateProduct(productCopy);
            _urlRecordService.SaveSlug(productCopy, seName, 0);

            var languages = _languageService.GetAllLanguages(true);

            //product tags
            foreach (var productTag in product.ProductTags)
            {
                productCopy.ProductTags.Add(productTag);
                productTag.ProductId = productCopy.Id;
                _productService.InsertProductTag(productTag);
            }

            //product pictures
            //variable to store original and new picture identifiers
            int id = 1;
            var originalNewPictureIdentifiers = new Dictionary<int, int>();
            if (copyImages)
            {
                foreach (var productPicture in product.ProductPictures)
                {
                    var picture = _pictureService.GetPictureById(productPicture.PictureId);
                    var pictureCopy = _pictureService.InsertPicture(
                        _pictureService.LoadPictureBinary(picture),
                        picture.MimeType,
                        _pictureService.GetPictureSeName(newName),
                        picture.AltAttribute,
                        picture.TitleAttribute);

                    _productService.InsertProductPicture(new ProductPicture
                    {
                        _id = ObjectId.GenerateNewId().ToString(),
                        Id = id,
                        ProductId = productCopy.Id,
                        PictureId = pictureCopy.Id,
                        DisplayOrder = productPicture.DisplayOrder
                    });
                    id++;
                    originalNewPictureIdentifiers.Add(picture.Id, pictureCopy.Id);
                }
            }

            // product <-> warehouses mappings
            foreach (var pwi in product.ProductWarehouseInventory)
            {
                var pwiCopy = new ProductWarehouseInventory
                {
                    Id = productCopy.ProductWarehouseInventory.Count > 0 ? productCopy.ProductWarehouseInventory.Max(x => x.Id) + 1 : 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                    ProductId = productCopy.Id,
                    WarehouseId = pwi.WarehouseId,
                    StockQuantity = pwi.StockQuantity,
                    ReservedQuantity = 0,
                };
                productCopy.ProductWarehouseInventory.Add(pwiCopy);
                _productService.InsertProductWarehouseInventory(pwiCopy);
            }

            // product <-> categories mappings
            foreach (var productCategory in product.ProductCategories)
            {
                var productCategoryCopy = new ProductCategory
                {
                    Id = productCopy.ProductCategories.Count > 0 ? productCopy.ProductCategories.Max(x => x.Id) + 1 : 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                    ProductId = productCopy.Id,
                    CategoryId = productCategory.CategoryId,
                    IsFeaturedProduct = productCategory.IsFeaturedProduct,
                    DisplayOrder = productCategory.DisplayOrder
                };
                productCopy.ProductCategories.Add(productCategoryCopy);
                _categoryService.InsertProductCategory(productCategoryCopy);
            }

            // product <-> manufacturers mappings
            foreach (var productManufacturers in product.ProductManufacturers)
            {
                var productManufacturerCopy = new ProductManufacturer
                {
                    Id = productCopy.ProductManufacturers.Count > 0 ? productCopy.ProductManufacturers.Max(x => x.Id) + 1 : 1,
                    _id = ObjectId.GenerateNewId().ToString(),
                    ProductId = productCopy.Id,
                    ManufacturerId = productManufacturers.ManufacturerId,
                    IsFeaturedProduct = productManufacturers.IsFeaturedProduct,
                    DisplayOrder = productManufacturers.DisplayOrder
                };
                productCopy.ProductManufacturers.Add(productManufacturerCopy);
                _manufacturerService.InsertProductManufacturer(productManufacturerCopy);
            }

            // product <-> releated products mappings
            foreach (var relatedProduct in product.RelatedProducts)
            {
                var copyrelatedProduct = (
                    new RelatedProduct
                    {
                        Id = relatedProduct.Id,
                        _id = relatedProduct._id,
                        ProductId1 = productCopy.Id,
                        ProductId2 = relatedProduct.ProductId2,
                        DisplayOrder = relatedProduct.DisplayOrder
                    });
                _productService.InsertRelatedProduct(copyrelatedProduct);
            }

            id = 1;
            // product <-> cross sells mappings
            foreach (var csProduct in product.CrossSellProduct) //_productService.GetCrossSellProductsByProductId1(product.Id, true))
            {
                _productService.InsertCrossSellProduct(
                    new CrossSellProduct
                    {
                        Id = id,
                        _id = ObjectId.GenerateNewId().ToString(),
                        ProductId1 = productCopy.Id,
                        ProductId2 = csProduct,
                    });
                id++;
            }

            // product specifications
            foreach (var productSpecificationAttribute in product.ProductSpecificationAttributes)
            {
                var psaCopy = new ProductSpecificationAttribute
                {
                    Id = productSpecificationAttribute.Id,
                    _id = productSpecificationAttribute._id,
                    ProductId = productCopy.Id,
                    AttributeTypeId = productSpecificationAttribute.AttributeTypeId,
                    SpecificationAttributeOptionId = productSpecificationAttribute.SpecificationAttributeOptionId,
                    CustomValue = productSpecificationAttribute.CustomValue,
                    AllowFiltering = productSpecificationAttribute.AllowFiltering,
                    ShowOnProductPage = productSpecificationAttribute.ShowOnProductPage,
                    DisplayOrder = productSpecificationAttribute.DisplayOrder
                };
                _specificationAttributeService.InsertProductSpecificationAttribute(psaCopy);
            }



            // product <-> attributes mappings
            var associatedAttributes = new Dictionary<int, int>();
            var associatedAttributeValues = new Dictionary<int, int>();
            foreach (var productAttributeMapping in product.ProductAttributeMappings) //_productAttributeService.GetProductAttributeMappingsByProductId(product.Id))
            {
                var productAttributeMappingCopy = new ProductAttributeMapping
                {
                    Id = productAttributeMapping.Id,
                    _id = productAttributeMapping._id,
                    ProductId = productCopy.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    IsRequired = productAttributeMapping.IsRequired,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    ValidationMinLength = productAttributeMapping.ValidationMinLength,
                    ValidationMaxLength = productAttributeMapping.ValidationMaxLength,
                    ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions,
                    ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    //UNDONE copy ConditionAttributeXml (we should replace attribute IDs with new values)
                };
                _productAttributeService.InsertProductAttributeMapping(productAttributeMappingCopy);
                //save associated value (used for combinations copying)
                associatedAttributes.Add(productAttributeMapping.Id, productAttributeMappingCopy.Id);

                // product attribute values
                var productAttributeValues = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMapping.Id).FirstOrDefault().ProductAttributeValues; //_productAttributeService.GetProductAttributeValues(productAttributeMapping.Id);
                foreach (var productAttributeValue in productAttributeValues)
                {
                    int attributeValuePictureId = 0;
                    if (originalNewPictureIdentifiers.ContainsKey(productAttributeValue.PictureId))
                    {
                        attributeValuePictureId = originalNewPictureIdentifiers[productAttributeValue.PictureId];
                    }
                    var attributeValueCopy = new ProductAttributeValue
                    {
                        Id = productAttributeValue.Id,
                        _id = productAttributeValue._id,
                        ProductAttributeMappingId = productAttributeMappingCopy.Id,
                        AttributeValueTypeId = productAttributeValue.AttributeValueTypeId,
                        AssociatedProductId = productAttributeValue.AssociatedProductId,
                        Name = productAttributeValue.Name,
                        ColorSquaresRgb = productAttributeValue.ColorSquaresRgb,
                        PriceAdjustment = productAttributeValue.PriceAdjustment,
                        WeightAdjustment = productAttributeValue.WeightAdjustment,
                        Cost = productAttributeValue.Cost,
                        Quantity = productAttributeValue.Quantity,
                        IsPreSelected = productAttributeValue.IsPreSelected,
                        DisplayOrder = productAttributeValue.DisplayOrder,
                        PictureId = attributeValuePictureId,
                        Locales = productAttributeMappingCopy.Locales
                    };
                    _productAttributeService.InsertProductAttributeValue(attributeValueCopy);

                    //save associated value (used for combinations copying)
                    associatedAttributeValues.Add(productAttributeValue.Id, attributeValueCopy.Id);
                }
            }
            //attribute combinations
            foreach (var combination in product.ProductAttributeCombinations)//_productAttributeService.GetAllProductAttributeCombinations(product.Id))
            {
                //generate new AttributesXml according to new value IDs
                string newAttributesXml = "";
                var parsedProductAttributes = _productAttributeParser.ParseProductAttributeMappings(product, combination.AttributesXml);
                foreach (var oldAttribute in parsedProductAttributes)
                {
                    if (associatedAttributes.ContainsKey(oldAttribute.Id))
                    {
                        var newAttribute = product.ProductAttributeMappings.Where(x => x.Id == associatedAttributes[oldAttribute.Id]).FirstOrDefault(); //_productAttributeService.GetProductAttributeMappingById(associatedAttributes[oldAttribute.Id]);
                        if (newAttribute != null)
                        {
                            var oldAttributeValuesStr = _productAttributeParser.ParseValues(combination.AttributesXml, oldAttribute.Id);
                            foreach (var oldAttributeValueStr in oldAttributeValuesStr)
                            {
                                if (newAttribute.ShouldHaveValues())
                                {
                                    //attribute values
                                    int oldAttributeValue = int.Parse(oldAttributeValueStr);
                                    if (associatedAttributeValues.ContainsKey(oldAttributeValue))
                                    {
                                        var newAttributeValue = newAttribute.ProductAttributeValues.Where(x => x.Id == associatedAttributeValues[oldAttributeValue]).FirstOrDefault(); //_productAttributeService.GetProductAttributeValueById(associatedAttributeValues[oldAttributeValue]);
                                        if (newAttributeValue != null)
                                        {
                                            newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                                newAttribute, newAttributeValue.Id.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    //just a text
                                    newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                        newAttribute, oldAttributeValueStr);
                                }
                            }
                        }
                    }
                }
                var combinationCopy = new ProductAttributeCombination
                {
                    ProductId = productCopy.Id,
                    AttributesXml = newAttributesXml,
                    StockQuantity = combination.StockQuantity,
                    AllowOutOfStockOrders = combination.AllowOutOfStockOrders,
                    Sku = combination.Sku,
                    ManufacturerPartNumber = combination.ManufacturerPartNumber,
                    Gtin = combination.Gtin,
                    OverriddenPrice = combination.OverriddenPrice,
                    NotifyAdminForQuantityBelow = combination.NotifyAdminForQuantityBelow,
                    Id = combination.Id, //productCopy.ProductAttributeCombinations.Count > 0 ? productCopy.ProductAttributeCombinations.Max(x=>x.Id) + 1 : 1,
                    _id = ObjectId.GenerateNewId().ToString()
                };
                _productAttributeService.InsertProductAttributeCombination(combinationCopy);
            }
            //tier prices
            foreach (var tierPrice in product.TierPrices)
            {
                _productService.InsertTierPrice(
                    new TierPrice
                    {
                        ProductId = productCopy.Id,
                        StoreId = tierPrice.StoreId,
                        CustomerRoleId = tierPrice.CustomerRoleId,
                        Quantity = tierPrice.Quantity,
                        Price = tierPrice.Price,
                        Id = tierPrice.Id,
                        _id = tierPrice._id
                    });
            }

            // product <-> discounts mapping
            foreach (var discount in product.AppliedDiscounts)
            {
                productCopy.AppliedDiscounts.Add(discount);
                _productService.InsertDiscount(discount, productCopy.Id);
            }


            //update "HasTierPrices" and "HasDiscountsApplied" properties
            _productService.UpdateHasTierPricesProperty(productCopy.Id);
            _productService.UpdateHasDiscountsApplied(productCopy.Id);


            //associated products
            if (copyAssociatedProducts)
            {
                var associatedProducts = _productService.GetAssociatedProducts(product.Id, showHidden: true);
                foreach (var associatedProduct in associatedProducts)
                {
                    var associatedProductCopy = CopyProduct(associatedProduct, string.Format("Copy of {0}", associatedProduct.Name),
                        isPublished, copyImages, false);
                    associatedProductCopy.ParentGroupedProductId = productCopy.Id;
                    _productService.UpdateProduct(productCopy);
                }
            }

            return productCopy;
        }

        #endregion
    }
}