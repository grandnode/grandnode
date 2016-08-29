using FluentValidation;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;
using Grand.Web.Models.ShoppingCart;

namespace Grand.Web.Validators.ShoppingCart
{
    public class WishlistEmailAFriendValidator : BaseNopValidator<WishlistEmailAFriendModel>
    {
        public WishlistEmailAFriendValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FriendEmail).NotEmpty().WithMessage(localizationService.GetResource("Wishlist.EmailAFriend.FriendEmail.Required"));
            RuleFor(x => x.FriendEmail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));

            RuleFor(x => x.YourEmailAddress).NotEmpty().WithMessage(localizationService.GetResource("Wishlist.EmailAFriend.YourEmailAddress.Required"));
            RuleFor(x => x.YourEmailAddress).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}