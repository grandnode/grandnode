using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Core.Domain.Catalog;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductValidator : BaseGrandValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));
            RuleFor(x => x.AuctionEnded && x.ProductTypeId == (int)ProductType.Auction).Equal(false).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Cannoteditauction"));
        }
    }
}