using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetReturnRequests : IRequest<CustomerReturnRequestsModel>
    { 
    }
}
