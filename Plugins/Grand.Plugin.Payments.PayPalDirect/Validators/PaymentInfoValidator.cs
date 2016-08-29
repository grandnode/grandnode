﻿using FluentValidation;
using Grand.Plugin.Payments.PayPalDirect.Models;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Plugin.Payments.PayPalDirect.Validators
{
    public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            //useful links:
            //http://fluentvalidation.codeplex.com/wikipage?title=Custom&referringTitle=Documentation&ANCHOR#CustomValidator
            //http://benjii.me/2010/11/credit-card-validator-attribute-for-asp-net-mvc-3/

            //RuleFor(x => x.CardNumber).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardNumber.Required"));
            //RuleFor(x => x.CardCode).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardCode.Required"));

            RuleFor(x => x.CardholderName).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardholderName.Required"));
            RuleFor(x => x.CardNumber).IsCreditCard().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));
            RuleFor(x => x.CardCode).Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResource("Payment.CardCode.Wrong"));
            RuleFor(x => x.ExpireMonth).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireMonth.Required"));
            RuleFor(x => x.ExpireYear).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireYear.Required"));
        }}
}