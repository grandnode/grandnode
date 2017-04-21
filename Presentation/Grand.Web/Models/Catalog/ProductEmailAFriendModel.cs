using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Catalog;

namespace Grand.Web.Models.Catalog
{
    [Validator(typeof(ProductEmailAFriendValidator))]
    public partial class ProductEmailAFriendModel : BaseGrandModel
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.EmailAFriend.FriendEmail")]
        public string FriendEmail { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.EmailAFriend.YourEmailAddress")]
        public string YourEmailAddress { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.EmailAFriend.PersonalMessage")]
        public string PersonalMessage { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}