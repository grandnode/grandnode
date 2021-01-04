using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Events
{
    public class CustomerInfoEvent : INotification
    {
        public Customer Customer { get; set; }
        public CustomerInfoModel Model { get; set; }
        public IFormCollection Form { get; set; }
        public IList<CustomAttribute> CustomerAttributes { get; set; }

        public CustomerInfoEvent(Customer customer, CustomerInfoModel model, IFormCollection form, IList<CustomAttribute> customerAttributes)
        {
            Customer = customer;
            Model = model;
            Form = form;
            CustomerAttributes = customerAttributes;
        }
    }
}
