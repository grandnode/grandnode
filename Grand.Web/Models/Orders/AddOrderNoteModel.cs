using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Orders
{
    [Validator(typeof(AddOrderNoteValidator))]
    public class AddOrderNoteModel : BaseGrandModel
    {
        public string OrderId { get; set; }
        public string Note { get; set; }
    }
}
