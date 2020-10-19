using Grand.Domain.Customers;
using Grand.Domain.Messages;
using MediatR;

namespace Grand.Web.Events
{
    public class PopupRenderEvent : INotification
    {
        public Customer Customer { get; private set; }
        public PopupActive PopupActive { get; private set; }
        public PopupRenderEvent(Customer customer, PopupActive popupActive)
        {
            Customer = customer;
            PopupActive = popupActive;
        }
    }
}
