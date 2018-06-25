using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductValidator : BaseGrandValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService, CommonSettings commonSettings)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));
            if(!commonSettings.AllowEditProductEndedAuction)
                RuleFor(x => x.AuctionEnded && x.ProductTypeId == (int)ProductType.Auction).Equal(false).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Cannoteditauction"));

            RuleFor(x => x.ProductTypeId == (int)ProductType.Auction && !x.AvailableEndDateTimeUtc.HasValue).Equal(false).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.AvailableEndDateTime.Required"));
        }
    }
}