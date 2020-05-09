using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Stores;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Stores
{
    public class StoreValidator : BaseGrandValidator<StoreModel>
    {
        public StoreValidator(
            IEnumerable<IValidatorConsumer<StoreModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Name.Required"));
            RuleFor(x => x.Shortcut).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Shortcut.Required"));
            RuleFor(x => x.Url).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Url.Required"));
        }
    }
}