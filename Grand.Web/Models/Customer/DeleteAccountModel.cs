using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;
using System.ComponentModel.DataAnnotations;

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