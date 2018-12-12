using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Customers;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(UserApiValidator))]
    public partial class UserApiModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.UserApi.Email")]

        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.System.UserApi.Password")]

        public string Password { get; set; }

        [GrandResourceDisplayName("Admin.System.UserApi.IsActive")]
        public bool IsActive { get; set; }

    }
}
