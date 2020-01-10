#! "netcoreapp3.1"
#r "Grand.Core"
#r "Grand.Framework"
#r "Grand.Services"
#r "Grand.Web"

using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Events;
using Grand.Web.Models.Common;
using System.Threading.Tasks;

/* Sample code to validate ZIP Code field in the Address */
public class ZipCodeValidation : IValidatorConsumer<AddressModel>
{
    public void AddRules(BaseGrandValidator<AddressModel> validator)
    {
        validator.RuleFor(x => x.ZipPostalCode).Matches(@"^[0-9]{2}\-[0-9]{3}$")
            .WithMessage("Provided zip code is invalid");
    }
}
