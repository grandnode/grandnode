using Grand.Domain.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Grand.Services.Commands.Handlers.Orders
{
    public class ActivatedValueForPurchasedGiftCardsCommandHandler : IRequestHandler<ActivatedValueForPurchasedGiftCardsCommand, bool>
    {
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;

        public ActivatedValueForPurchasedGiftCardsCommandHandler(
            IGiftCardService giftCardService,
            ILanguageService languageService,
            IWorkflowMessageService workflowMessageService)
        {
            _giftCardService = giftCardService;
            _languageService = languageService;
            _workflowMessageService = workflowMessageService;
        }

        public async Task<bool> Handle(ActivatedValueForPurchasedGiftCardsCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            foreach (var orderItem in request.Order.OrderItems)
            {
                var giftCards = await _giftCardService.GetAllGiftCards(purchasedWithOrderItemId: orderItem.Id,
                    isGiftCardActivated: !request.Activate);
                foreach (var gc in giftCards)
                {
                    if (request.Activate)
                    {
                        //activate
                        bool isRecipientNotified = gc.IsRecipientNotified;
                        if (gc.GiftCardType == GiftCardType.Virtual)
                        {
                            //send email for virtual gift card
                            if (!String.IsNullOrEmpty(gc.RecipientEmail) &&
                                !String.IsNullOrEmpty(gc.SenderEmail))
                            {
                                var customerLang = await _languageService.GetLanguageById(request.Order.CustomerLanguageId);
                                if (customerLang == null)
                                    customerLang = (await _languageService.GetAllLanguages()).FirstOrDefault();
                                if (customerLang == null)
                                    throw new Exception("No languages could be loaded");
                                int queuedEmailId = await _workflowMessageService.SendGiftCardNotification(gc, request.Order, customerLang.Id);
                                if (queuedEmailId > 0)
                                    isRecipientNotified = true;
                            }
                        }
                        gc.IsGiftCardActivated = true;
                        gc.IsRecipientNotified = isRecipientNotified;
                        await _giftCardService.UpdateGiftCard(gc);
                    }
                    else
                    {
                        //deactivate
                        gc.IsGiftCardActivated = false;
                        await _giftCardService.UpdateGiftCard(gc);
                    }
                }
            }

            return true;
        }
    }
}
