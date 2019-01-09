using FluentValidation;
using Grand.Api.DTOs.Customers;
using Grand.Core.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using System;

namespace Grand.Api.Validators.Customers
{
    public class CustomerRoleValidator : BaseGrandValidator<CustomerRoleDto>
    {
        public CustomerRoleValidator(ILocalizationService localizationService, IProductService productService, ICustomerService customerService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Customers.CustomerRole.Fields.Name.Required"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.PurchasedWithProductId))
                {
                    var product = productService.GetProductById(x.PurchasedWithProductId);
                    if (product == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.CustomerRole.Fields.PurchasedWithProductId.NotExists"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var role = customerService.GetCustomerRoleById(x.Id);
                    if (role == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.CustomerRole.Fields.Id.NotExists"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var customerRole = customerService.GetCustomerRoleById(x.Id);
                    if (customerRole.IsSystemRole && !x.Active)
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.CustomerRoles.Fields.Active.CantEditSystem"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var customerRole = customerService.GetCustomerRoleById(x.Id);
                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(x.SystemName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var customerRole = customerService.GetCustomerRoleById(x.Id);
                    if (SystemCustomerRoleNames.Registered.Equals(customerRole.SystemName, StringComparison.OrdinalIgnoreCase) &&
                            !String.IsNullOrEmpty(x.PurchasedWithProductId))
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Customers.CustomerRoles.Fields.PurchasedWithProduct.Registered"));
        }
    }
}
