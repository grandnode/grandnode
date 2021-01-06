using Grand.Domain.Common;
using Grand.Domain.Customers;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Commands.Models.Customers
{
    public class CustomerActionEventConditionCommand : IRequest<bool>
    {
        public CustomerActionEventConditionCommand()
        {
            CustomerActionTypes = new List<CustomerActionType>();
        }
        public IList<CustomerActionType> CustomerActionTypes { get; set; }
        public CustomerAction Action { get; set; }
        public string ProductId { get; set; }
        public IList<CustomAttribute> Attributes { get; set; } = new List<CustomAttribute>();
        public string CustomerId { get; set; }
        public string CurrentUrl { get; set; }
        public string PreviousUrl { get; set; }
    }
}
