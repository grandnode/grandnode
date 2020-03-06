using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Order;
using MediatR;

namespace Grand.Web.Commands.Models
{
    public class InsertOrderNoteCommandModel : IRequest<OrderNote>
    {
        public Language Language { get; set; }
        public AddOrderNoteModel OrderNote { get; set; }
    }
}
