using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.ShoppingCart;
using System.Collections.Generic;

namespace Grand.Web.Validators.ShoppingCart
{
    public class WishlistEmailAFriendValidator : BaseGrandValidator<WishlistEmailAFriendModel>
    {
        public WishlistEmailAFriendValidator(
            IEnumerable<IValidatorConsumer<WishlistEmailAFriendModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.FriendEmail).NotEmpty().WithMessage(localizationService.GetResource("Wishlist.EmailAFriend.FriendEmail.Required"));
            RuleFor(x => x.FriendEmail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));

            RuleFor(x => x.YourEmailAddress).NotEmpty().WithMessage(localizationService.GetResource("Wishlist.EmailAFriend.YourEmailAddress.Required"));
            RuleFor(x => x.YourEmailAddress).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}