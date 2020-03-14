using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Commands.Models.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grand.Services.Orders;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ApplyGiftCardCommandHandler : IRequestHandler<ApplyGiftCardCommand, bool>
    {
        private readonly IGenericAttributeService _genericAttributeService;

        public ApplyGiftCardCommandHandler(IGenericAttributeService genericAttributeService)
        {
            _genericAttributeService = genericAttributeService;
        }

        public async Task<bool> Handle(ApplyGiftCardCommand request, CancellationToken cancellationToken)
        {
            if (request.Customer == null)
                throw new ArgumentNullException("customer");

            var result = GiftCardExtensions.ApplyCouponCode(request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.GiftCardCouponCodes),
                request.GiftCardCouponCode.Trim().ToLower());

            //apply new value
            await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.GiftCardCouponCodes, result);

            return true;
        }


    }
}
