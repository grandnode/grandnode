using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Orders
{
    public class AddOrderNoteModel : BaseGrandModel
    {
        public string OrderId { get; set; }
        public string Note { get; set; }
    }
}
