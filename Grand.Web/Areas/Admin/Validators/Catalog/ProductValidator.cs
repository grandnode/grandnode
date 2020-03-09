using FluentValidation;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductValidator : BaseGrandValidator<ProductModel>
    {
        public ProductValidator(
            IEnumerable<IValidatorConsumer<ProductModel>> validators,
            ILocalizationService localizationService, CommonSettings commonSettings)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));
            if(!commonSettings.AllowEditProductEndedAuction)
                RuleFor(x => x.AuctionEnded && x.ProductTypeId == (int)ProductType.Auction).Equal(false).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Cannoteditauction"));

            RuleFor(x => x.ProductTypeId == (int)ProductType.Auction && !x.AvailableEndDateTime.HasValue).Equal(false).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.AvailableEndDateTime.Required"));
        }
    }
}