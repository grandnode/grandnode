using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Stores;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ProductTierPriceValidator : BaseGrandValidator<ProductTierPriceDto>
    {
        public ProductTierPriceValidator(
            IEnumerable<IValidatorConsumer<ProductTierPriceDto>> validators,
            ILocalizationService localizationService, IStoreService storeService, ICustomerService customerService)
            : base(validators)
        {
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage(localizationService.GetResource("Api.Catalog.ProductTierPrice.Fields.Quantity.GreaterThan0"));
            RuleFor(x => x.Price).GreaterThan(0).WithMessage(localizationService.GetResource("Api.Catalog.ProductTierPrice.Fields.Price.GreaterThan0"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.StoreId))
                {
                    var store = await storeService.GetStoreById(x.StoreId);
                    if (store == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductTierPrice.Fields.StoreId.NotExists"));
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.CustomerRoleId))
                {
                    var role = await customerService.GetCustomerRoleById(x.CustomerRoleId);
                    if (role == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductTierPrice.Fields.CustomerRoleId.NotExists"));
        }
    }
}
