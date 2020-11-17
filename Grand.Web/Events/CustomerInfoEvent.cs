using Grand.Domain.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Events
{
    public class CustomerInfoEvent : INotification
    {
        public Customer Customer { get; set; }
        public CustomerInfoModel Model { get; set; }
        public IFormCollection Form { get; set; }
        public string CustomerAttributesXml { get; set; }

        public CustomerInfoEvent(Customer customer, CustomerInfoModel model, IFormCollection form, string customerAttributesXml)
        {
            Customer = customer;
            Model = model;
            Form = form;
            CustomerAttributesXml = customerAttributesXml;
        }
    }
}
