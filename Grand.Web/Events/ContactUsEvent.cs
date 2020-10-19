using Grand.Domain.Customers;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Events
{
    public class ContactUsEvent : INotification
    {
        public Customer Customer { get; private set; }
        public ContactUsModel Model { get; private set; }
        public IFormCollection Form { get; private set; }

        public ContactUsEvent(Customer customer, ContactUsModel model, IFormCollection form)
        {
            Customer = customer;
            Model = model;
            Form = form;
        }
    }
}
