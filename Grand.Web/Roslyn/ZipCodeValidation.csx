#! "netcoreapp2.1"
#r "Grand.Core"
#r "Grand.Framework"
#r "Grand.Services"
#r "Grand.Web"

using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Events;
using Grand.Web.Models.Common;

/* Sample code to validate ZIP Code field in the Address */
public class ZipCodeValidation : IConsumer<BaseGrandValidator<AddressModel>>
{
    public void HandleEvent(BaseGrandValidator<AddressModel> eventMessage)
    {
        eventMessage.RuleFor(x => x.ZipPostalCode).Matches(@"^[0-9]{2}\-[0-9]{3}$")
            .WithMessage("Provided zip code is invalid");
    }
}
