using Grand.Web.Models.Customer;
using MediatR;
using System;

namespace Grand.Web.Features.Models.Customers
{
    public class GetUserAgreement : IRequest<UserAgreementModel>
    {
        public Guid OrderItemId { get; set; }
    }
}
