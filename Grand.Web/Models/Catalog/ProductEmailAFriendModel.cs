using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Catalog;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Catalog
{
    [Validator(typeof(ProductEmailAFriendValidator))]
    public partial class ProductEmailAFriendModel : BaseGrandModel
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        [GrandResourceDisplayName("Products.EmailAFriend.FriendEmail")]
        public string FriendEmail { get; set; }

        [GrandResourceDisplayName("Products.EmailAFriend.YourEmailAddress")]
        public string YourEmailAddress { get; set; }

        [GrandResourceDisplayName("Products.EmailAFriend.PersonalMessage")]
        public string PersonalMessage { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}