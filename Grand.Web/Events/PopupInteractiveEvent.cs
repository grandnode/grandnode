using Grand.Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Events
{
    public class PopupInteractiveEvent : INotification
    {
        public IFormCollection Form { get; private set; }
        public IList<(string attrName, string attrValue)> EnquiryForm { get; private set; }
        public Customer Customer { get; private set; }
        public PopupInteractiveEvent(
            Customer customer,
            IFormCollection form,
            IList<(string attrName, string attrValue)> enquiryForm)
        {
            Customer = customer;
            Form = form;
            EnquiryForm = enquiryForm;
        }
    }
}
