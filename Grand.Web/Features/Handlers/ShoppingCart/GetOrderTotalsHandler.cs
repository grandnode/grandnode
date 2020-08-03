using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Tax;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetOrderTotalsHandler : IRequestHandler<GetOrderTotals, OrderTotalsModel>
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentService _paymentService;
        private readonly ITaxService _taxService;
        private readonly IMediator _mediator;

        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        public GetOrderTotalsHandler(
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IPaymentService paymentService,
            ITaxService taxService,
            IMediator mediator,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _paymentService = paymentService;
            _taxService = taxService;
            _mediator = mediator;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
        }

        public async Task<OrderTotalsModel> Handle(GetOrderTotals request, CancellationToken cancellationToken)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = request.IsEditable;

            if (request.Cart.Any())
            {
                //subtotal
                await PrepareSubtotal(model, request);

                //shipping info
                await PrepareShippingInfo(model, request);

                //payment method fee
                await PreparePayment(model, request);

                //tax
                await PrepareTax(model, request);

                //total
                await PrepareTotal(model, request);
            }
            return model;
        }

        private async Task PrepareSubtotal(OrderTotalsModel model, GetOrderTotals request)
        {
            var subTotalIncludingTax = request.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(request.Cart, subTotalIncludingTax);
            decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
            decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
            decimal subtotalBase = subTotalWithoutDiscountBase;
            decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, request.Currency);
            model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, request.Currency, request.Language, subTotalIncludingTax);

            if (orderSubTotalDiscountAmountBase > decimal.Zero)
            {
                decimal orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, request.Currency);
                model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, request.Currency, request.Language, subTotalIncludingTax);
            }
        }

        private async Task PrepareShippingInfo(OrderTotalsModel model, GetOrderTotals request)
        {
            model.RequiresShipping = request.Cart.RequiresShipping();
            if (model.RequiresShipping)
            {
                decimal? shoppingCartShippingBase = (await _orderTotalCalculationService.GetShoppingCartShippingTotal(request.Cart)).shoppingCartShippingTotal;
                if (shoppingCartShippingBase.HasValue)
                {
                    decimal shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, request.Currency);
                    model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                    //selected shipping method
                    var shippingOption = request.Customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, request.Store.Id);
                    if (shippingOption != null)
                        model.SelectedShippingMethod = shippingOption.Name;
                }
            }

        }

        private async Task PreparePayment(OrderTotalsModel model, GetOrderTotals request)
        {
            var paymentMethodSystemName = request.Customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, request.Store.Id);
            decimal paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFee(request.Cart, paymentMethodSystemName);
            decimal paymentMethodAdditionalFeeWithTaxBase = (await _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, request.Customer)).paymentPrice;
            if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
            {
                decimal paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, request.Currency);
                model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
            }
        }

        private async Task PrepareTax(OrderTotalsModel model, GetOrderTotals request)
        {
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && request.TaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var taxtotal = await _orderTotalCalculationService.GetTaxTotal(request.Cart);
                SortedDictionary<decimal, decimal> taxRates = taxtotal.taxRates;
                decimal shoppingCartTaxBase = taxtotal.taxtotal;
                decimal shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, request.Currency);

                if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                    foreach (var tr in taxRates)
                    {
                        model.TaxRates.Add(new OrderTotalsModel.TaxRate {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            Value = _priceFormatter.FormatPrice(await _currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, request.Currency), true, false),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
        }

        private async Task PrepareTotal(OrderTotalsModel model, GetOrderTotals request)
        {
            var carttotal = await _orderTotalCalculationService.GetShoppingCartTotal(request.Cart);
            decimal? shoppingCartTotalBase = carttotal.shoppingCartTotal;
            decimal orderTotalDiscountAmountBase = carttotal.discountAmount;
            List<AppliedGiftCard> appliedGiftCards = carttotal.appliedGiftCards;
            int redeemedRewardPoints = carttotal.redeemedRewardPoints;
            decimal redeemedRewardPointsAmount = carttotal.redeemedRewardPointsAmount;
            if (shoppingCartTotalBase.HasValue)
            {
                decimal shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, request.Currency);
                model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
            }
            //discount
            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                decimal orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, request.Currency);
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
            }

            //gift cards
            if (appliedGiftCards != null && appliedGiftCards.Any())
            {
                foreach (var appliedGiftCard in appliedGiftCards)
                {
                    await PrepareGiftCards(appliedGiftCard, model, request);
                }
            }

            //reward points to be spent (redeemed)
            if (redeemedRewardPointsAmount > decimal.Zero)
            {
                decimal redeemedRewardPointsAmountInCustomerCurrency = await _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, request.Currency);
                model.RedeemedRewardPoints = redeemedRewardPoints;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
            }

            //reward points to be earned
            if (_rewardPointsSettings.Enabled &&
                _rewardPointsSettings.DisplayHowMuchWillBeEarned &&
                shoppingCartTotalBase.HasValue)
            {
                decimal? shippingBaseInclTax = model.RequiresShipping
                    ? (await _orderTotalCalculationService.GetShoppingCartShippingTotal(request.Cart, true)).shoppingCartShippingTotal
                    : 0;
                var earnRewardPoints = shoppingCartTotalBase.Value - shippingBaseInclTax.Value;
                if (earnRewardPoints > 0)
                    model.WillEarnRewardPoints = await _mediator.Send(new CalculateRewardPointsCommand() { Customer = request.Customer, Amount = earnRewardPoints });
            }
        }
        
        private async Task PrepareGiftCards(AppliedGiftCard appliedGiftCard, OrderTotalsModel model, GetOrderTotals request)
        {
            var gcModel = new OrderTotalsModel.GiftCard {
                Id = appliedGiftCard.GiftCard.Id,
                CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
            };
            decimal amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, request.Currency);
            gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

            decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
            decimal remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, request.Currency);
            gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);

            model.GiftCards.Add(gcModel);
        }
        
    }
}
