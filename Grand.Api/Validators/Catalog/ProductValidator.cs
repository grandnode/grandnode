using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Vendors;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ProductValidator : BaseGrandValidator<ProductDto>
    {
        public ProductValidator(
            IEnumerable<IValidatorConsumer<ProductDto>> validators,
            ILocalizationService localizationService, IProductService productService, IProductTemplateService productTemplateService, IVendorService vendorService, CommonSettings commonSettings)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.Name.Required"));
            RuleFor(x => x.ProductType).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.ProductType.Required"));
            RuleFor(x => x.BackorderMode).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.BackorderMode.Required"));
            RuleFor(x => x.DownloadActivationType).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.DownloadActivationType.Required"));
            RuleFor(x => x.IntervalUnitType).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.IntervalUnitType.Required"));
            RuleFor(x => x.GiftCardType).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.GiftCardType.Required"));
            RuleFor(x => x.LowStockActivity).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.LowStockActivity.Required"));
            RuleFor(x => x.ManageInventoryMethod).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.ManageInventoryMethod.Required"));
            RuleFor(x => x.RecurringCyclePeriod).IsInEnum().WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.RecurringCyclePeriod.Required"));

            if (!commonSettings.AllowEditProductEndedAuction)
                RuleFor(x => x.AuctionEnded && x.ProductType == ProductType.Auction).Equal(false).WithMessage(localizationService.GetResource("Api.Catalog.Products.Cannoteditauction"));

            RuleFor(x => x.ProductType == ProductType.Auction && !x.AvailableEndDateTimeUtc.HasValue).Equal(false).WithMessage(localizationService.GetResource("Api.Catalog.Products.Fields.AvailableEndDateTime.Required"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.ParentGroupedProductId))
                {
                    var product = await productService.GetProductById(x.ParentGroupedProductId);
                    if (product == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.ParentGroupedProductId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.ProductTemplateId))
                {
                    var template = await productTemplateService.GetProductTemplateById(x.ProductTemplateId);
                    if (template == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.ProductTemplateId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.VendorId))
                {
                    var vendor = await vendorService.GetVendorById(x.VendorId);
                    if (vendor == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.VendorId.NotExists"));


            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var product = await productService.GetProductById(x.Id);
                    if (product == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Product.Fields.Id.NotExists"));

        }
    }
}
