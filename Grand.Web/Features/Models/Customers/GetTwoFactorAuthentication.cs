using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetTwoFactorAuthentication : IRequest<CustomerInfoModel.TwoFactorAuthenticationModel>
    {
    }
}
