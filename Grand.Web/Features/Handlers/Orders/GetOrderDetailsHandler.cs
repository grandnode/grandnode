using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Shipping;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Media;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetOrderDetailsHandler : IRequestHandler<GetOrderDetails, OrderDetailsModel>
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IShipmentService _shipmentService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;

        public GetOrderDetailsHandler(
            IDateTimeHelper dateTimeHelper, 
            IProductService productService, 
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService,
            IShipmentService shipmentService, 
            IPaymentService paymentService,
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IGiftCardService giftCardService, 
            IOrderService orderService,
            IPictureService pictureService, 
            IDownloadService downloadService, 
            IMediator mediator,
            CatalogSettings catalogSettings, 
            OrderSettings orderSettings, 
            PdfSettings pdfSettings, 
            TaxSettings taxSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _localizationService = localizationService;
            _shipmentService = shipmentService;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _giftCardService = giftCardService;
            _orderService = orderService;
            _pictureService = pictureService;
            _downloadService = downloadService;
            _mediator = mediator;
            _orderSettings = orderSettings;
            _catalogSettings = catalogSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
        }

        public async Task<OrderDetailsModel> Handle(GetOrderDetails request, CancellationToken cancellationToken)
        {
            var model = new OrderDetailsModel();

            model.Id = request.Order.Id;
            model.OrderNumber = request.Order.OrderNumber;
            model.OrderCode = request.Order.Code;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.Order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = request.Order.OrderStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = await _mediator.Send(new IsReturnRequestAllowedQuery() { Order = request.Order });
            model.PdfInvoiceDisabled = _pdfSettings.DisablePdfInvoicesForPendingOrders && request.Order.OrderStatus == OrderStatus.Pending;
            model.ShowAddOrderNote = _orderSettings.AllowCustomerToAddOrderNote;

            //shipping info
            await PrepareShippingInfo(request, model);

            //billing info
            model.BillingAddress = await _mediator.Send(new GetAddressModel() {
                Language = request.Language,
                Model = null,
                Address = request.Order.BillingAddress,
                ExcludeProperties = false,
            });

            //VAT number
            model.VatNumber = request.Order.VatNumber;

            //payment method
            await PreparePaymentMethod(request, model);

            //custom values
            model.CustomValues = request.Order.DeserializeCustomValues();

            //order subtotal
            await PrepareOrderTotal(request, model);

            //tax
            await PrepareTax(request, model);

            //discount (applied to order total)
            await PrepareDiscount(request, model);

            //gift cards
            await PrepareGiftCards(request, model);

            //reward points           
            await PrepareRewardPoints(request, model);

            //checkout attributes
            model.CheckoutAttributeInfo = request.Order.CheckoutAttributeDescription;

            //order notes
            await PrepareOrderNotes(request, model);

            //allow cancel order
            if (_orderSettings.UserCanCancelUnpaidOrder)
            {
                if (request.Order.OrderStatus == OrderStatus.Pending && request.Order.PaymentStatus == Domain.Payments.PaymentStatus.Pending
                    && (request.Order.ShippingStatus == ShippingStatus.ShippingNotRequired || request.Order.ShippingStatus == ShippingStatus.NotYetShipped))
                    model.UserCanCancelUnpaidOrder = true;
            }

            //purchased products
            await PrepareOrderItems(request, model);
            return model;

        }

        private async Task PrepareShippingInfo(GetOrderDetails request, OrderDetailsModel model)
        {
            model.ShippingStatus = request.Order.ShippingStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            if (request.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickUpInStore = request.Order.PickUpInStore;
                if (!request.Order.PickUpInStore)
                {
                    model.ShippingAddress = await _mediator.Send(new GetAddressModel() {
                        Language = request.Language,
                        Model = null,
                        Address = request.Order.ShippingAddress,
                        ExcludeProperties = false,
                    });
                }
                else
                {
                    if (request.Order.PickupPoint != null)
                    {
                        if (request.Order.PickupPoint.Address != null)
                        {
                            model.PickupAddress = await _mediator.Send(new GetAddressModel() {
                                Language = request.Language,
                                Address = request.Order.PickupPoint.Address,
                                ExcludeProperties = false,
                            });
                        }
                    }
                }
                model.ShippingMethod = request.Order.ShippingMethod;
                model.ShippingAdditionDescription = request.Order.ShippingOptionAttributeDescription;
                //shipments (only already shipped)
                var shipments = (await _shipmentService.GetShipmentsByOrder(request.Order.Id)).Where(x => x.ShippedDateUtc.HasValue).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel {
                        Id = shipment.Id,
                        ShipmentNumber = shipment.ShipmentNumber,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }

        }

        private async Task PreparePaymentMethod(GetOrderDetails request, OrderDetailsModel model)
        {
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(request.Order.PaymentMethodSystemName);
            model.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, request.Language.Id) : request.Order.PaymentMethodSystemName;
            model.PaymentMethodStatus = request.Order.PaymentStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            model.CanRePostProcessPayment = await _paymentService.CanRePostProcessPayment(request.Order);
        }

        private async Task PrepareOrderTotal(GetOrderDetails request, OrderDetailsModel model)
        {
            if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //order subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderSubtotalInclTax, request.Order.CurrencyRate);
                model.OrderSubtotal = await _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderSubTotalDiscountInclTax, request.Order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = await _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
            }
            else
            {
                //excluding tax

                //order subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderSubtotalExclTax, request.Order.CurrencyRate);
                model.OrderSubtotal = await _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderSubTotalDiscountExclTax, request.Order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = await _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
            }

            if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //order shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderShippingInclTax, request.Order.CurrencyRate);
                model.OrderShipping = await _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.PaymentMethodAdditionalFeeInclTax, request.Order.CurrencyRate);
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
            }
            else
            {
                //excluding tax

                //order shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderShippingExclTax, request.Order.CurrencyRate);
                model.OrderShipping = await _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.PaymentMethodAdditionalFeeExclTax, request.Order.CurrencyRate);
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderTotal, request.Order.CurrencyRate);
            model.OrderTotal = await _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, request.Order.CustomerCurrencyCode, false, request.Language);

        }

        private async Task PrepareTax(GetOrderDetails request, OrderDetailsModel model)
        {
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (request.Order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && request.Order.TaxRatesDictionary.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderTax, request.Order.CurrencyRate);
                    //TODO pass languageId to _priceFormatter.FormatPrice
                    model.Tax = await _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, false, request.Language);

                    foreach (var tr in request.Order.TaxRatesDictionary)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            //TODO pass languageId to _priceFormatter.FormatPrice
                            Value = await _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, request.Order.CurrencyRate), true, request.Order.CustomerCurrencyCode, false, request.Language),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

        }

        private async Task PrepareDiscount(GetOrderDetails request, OrderDetailsModel model)
        {
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(request.Order.OrderDiscount, request.Order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = await _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, request.Order.CustomerCurrencyCode, false, request.Language);
        }

        private async Task PrepareGiftCards(GetOrderDetails request, OrderDetailsModel model)
        {
            foreach (var gcuh in await _giftCardService.GetAllGiftCardUsageHistory(request.Order.Id))
            {
                var giftCard = await _giftCardService.GetGiftCardById(gcuh.GiftCardId);
                model.GiftCards.Add(new OrderDetailsModel.GiftCard {
                    CouponCode = giftCard.GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, request.Order.CurrencyRate)), true, request.Order.CustomerCurrencyCode, false, request.Language),
                });
            }

        }

        private async Task PrepareRewardPoints(GetOrderDetails request, OrderDetailsModel model)
        {
            if (request.Order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -request.Order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(request.Order.RedeemedRewardPointsEntry.UsedAmount, request.Order.CurrencyRate)), true, request.Order.CustomerCurrencyCode, false, request.Language);
            }

        }

        private async Task PrepareOrderNotes(GetOrderDetails request, OrderDetailsModel model)
        {
            foreach (var orderNote in (await _orderService.GetOrderNotes(request.Order.Id))
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote {
                    Id = orderNote.Id,
                    OrderId = orderNote.OrderId,
                    HasDownload = !String.IsNullOrEmpty(orderNote.DownloadId),
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

        }

        private async Task PrepareOrderItems(GetOrderDetails request, OrderDetailsModel model)
        {
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var orderItem in request.Order.OrderItems)
            {
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                var orderItemModel = new OrderDetailsModel.OrderItemModel {
                    Id = orderItem.Id,
                    OrderItemGuid = orderItem.OrderItemGuid,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.SeName,
                    Quantity = orderItem.Quantity,
                    AttributeInfo = orderItem.AttributeDescription,
                };
                //prepare picture
                orderItemModel.Picture = await PrepareOrderItemPicture(product, orderItem.AttributesXml, orderItemModel.ProductName);

                model.Items.Add(orderItemModel);

                //unit price, subtotal
                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, request.Order.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceValue = unitPriceInclTaxInCustomerCurrency;

                    var unitPriceWithDiscInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscInclTax, request.Order.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = await _priceFormatter.FormatPrice(unitPriceWithDiscInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceWithDiscInclTaxInCustomerCurrency;

                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, request.Order.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    if (orderItem.DiscountAmountInclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountInclTax, request.Order.CurrencyRate);
                        orderItemModel.Discount = await _priceFormatter.FormatPrice(discountCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    }
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, request.Order.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
                    orderItemModel.UnitPriceValue = unitPriceExclTaxInCustomerCurrency;

                    var unitPriceExclWithDiscTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscExclTax, request.Order.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = await _priceFormatter.FormatPrice(unitPriceExclWithDiscTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceExclWithDiscTaxInCustomerCurrency;

                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, request.Order.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, false);
                    if (orderItem.DiscountAmountExclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountExclTax, request.Order.CurrencyRate);
                        orderItemModel.Discount = await _priceFormatter.FormatPrice(discountCustomerCurrency, true, request.Order.CustomerCurrencyCode, request.Language, true);
                    }
                }

                //downloadable products
                if (_downloadService.IsDownloadAllowed(request.Order, orderItem, product))
                    orderItemModel.DownloadId = product.DownloadId;
                if (_downloadService.IsLicenseDownloadAllowed(request.Order, orderItem, product))
                    orderItemModel.LicenseId = !string.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "";
            }

        }

        private async Task<PictureModel> PrepareOrderItemPicture(Product product, string attributesXml, string productName)
        {
            var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
            return new PictureModel {
                Id = sciPicture?.Id,
                ImageUrl = await _pictureService.GetPictureUrl(sciPicture, 80),
                Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
            };
        }


    }
}
