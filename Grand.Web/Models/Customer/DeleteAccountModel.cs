using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(DeleteAccountValidator))]
    public partial class DeleteAccountModel : BaseGrandModel
    {
        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.DeleteAccount.Fields.Password")]
        public string Password { get; set; }

    }
}