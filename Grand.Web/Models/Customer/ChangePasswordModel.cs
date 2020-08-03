using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    public partial class ChangePasswordModel : BaseGrandModel
    {
        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.ChangePassword.Fields.OldPassword")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.ChangePassword.Fields.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.ChangePassword.Fields.ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }

        public string Result { get; set; }

    }
}