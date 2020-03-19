using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Orders;
using Grand.Services.Commands.Models.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class RemoveGiftCardCommandHandler : IRequestHandler<RemoveGiftCardCommand, bool>
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;

        public RemoveGiftCardCommandHandler(IGenericAttributeService genericAttributeService, IGiftCardService giftCardService)
        {
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
        }

        public async Task<bool> Handle(RemoveGiftCardCommand request, CancellationToken cancellationToken)
        {
            if (request.Customer == null)
                throw new ArgumentNullException("customer");

            var giftCard = await _giftCardService.GetGiftCardById(request.GiftCardId);
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            //get applied coupon codes
            var existingCouponCodes = request.Customer.ParseAppliedGiftCardCouponCodes();

            //clear them
            await _genericAttributeService.SaveAttribute<string>(request.Customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(giftCard.GiftCardCouponCode, StringComparison.OrdinalIgnoreCase))
                {
                    var result = GiftCardExtensions.ApplyCouponCode(request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.GiftCardCouponCodes),
                            existingCouponCode);
                    await _genericAttributeService.SaveAttribute(request.Customer, SystemCustomerAttributeNames.GiftCardCouponCodes, result);
                }

            return true;

        }


    }
}
