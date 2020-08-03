using System;
using System.Collections.Generic;
using System.Text;
using Grand.Domain.Orders;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class CheckOrderStatusCommand : IRequest<bool>
    {
        public Order Order { get; set; }
    }
}
