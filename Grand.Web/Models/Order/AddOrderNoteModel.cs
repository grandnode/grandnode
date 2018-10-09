using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Order
{
    [Validator(typeof(AddOrderNoteValidator))]
    public class AddOrderNoteModel : BaseGrandModel
    {
        public string Note { get; set; }
    }
}
