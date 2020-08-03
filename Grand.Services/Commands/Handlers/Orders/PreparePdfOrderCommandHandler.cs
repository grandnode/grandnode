using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Core.Html;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Stores;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class PreparePdfOrderCommandHandler : IRequestHandler<PreparePdfOrderCommand, bool>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingContext;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IServiceProvider _serviceProvider;
        private readonly CatalogSettings _catalogSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;

        public PreparePdfOrderCommandHandler(
            ILocalizationService localizationService,
            ILanguageService languageService,
            IWorkContext workContext,
            IOrderService orderService,
            IPaymentService paymentService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingContext,
            IAddressAttributeFormatter addressAttributeFormatter,
            IServiceProvider serviceProvider,
            CatalogSettings catalogSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            AddressSettings addressSettings)
        {
            _localizationService = localizationService;
            _languageService = languageService;
            _workContext = workContext;
            _orderService = orderService;
            _paymentService = paymentService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _pictureService = pictureService;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _storeService = storeService;
            _storeContext = storeContext;
            _settingContext = settingContext;
            _addressAttributeFormatter = addressAttributeFormatter;
            _serviceProvider = serviceProvider;
            _catalogSettings = catalogSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _addressSettings = addressSettings;
        }

        public async Task<bool> Handle(PreparePdfOrderCommand request, CancellationToken cancellationToken)
        {
            await PreparePdfOrder(request.Doc, request.PdfWriter, request.PageSize, request.Order, request.LanguageId, request.VendorId);
            return true;

        }
        protected virtual async Task PreparePdfOrder(Document doc, PdfWriter pdfWriter, Rectangle pageSize, Order order, string languageId = "", string vendorId = "")
        {
            //by default _pdfSettings contains settings for the current active store
            //and we need PdfSettings for the store which was used to place an order
            //so let's load it based on a store of the current order
            var pdfSettingsByStore = _settingContext.LoadSetting<PdfSettings>(order.StoreId);

            var lang = await _languageService.GetLanguageById(languageId == "" ? order.CustomerLanguageId : languageId);
            if (lang == null || !lang.Published)
                lang = _workContext.WorkingLanguage;

            #region Header

            var headerTable = await PrepareOrderHeader(order, lang, pdfSettingsByStore);
            doc.Add(headerTable);

            #endregion

            #region Addresses

            var addressTable = await PrepareOrderAddressHeader(order, lang, pdfSettingsByStore);
            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));

            #endregion

            #region Products

            var productsHeader = new PdfPTable(1);
            productsHeader.RunDirection = GetDirection(lang);
            productsHeader.WidthPercentage = 100f;
            var cellProducts = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.Product(s)", lang.Id), PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName)));
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            var productsTable = await PrepareOrderProducts(order, lang, vendorId);
            doc.Add(productsTable);

            #endregion

            #region Checkout attributes

            if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription) && string.IsNullOrEmpty(vendorId))
            {
                var attribTable = PrepareOrderCheckoutattributes(order, lang);
                doc.Add(new Paragraph(" "));
                doc.Add(attribTable);
            }

            #endregion

            #region Totals
            if (string.IsNullOrEmpty(vendorId))
            {
                var totalsTable = await PrepareOrderTotal(order, lang);
                doc.Add(totalsTable);
            }

            #endregion

            #region Order notes

            if (pdfSettingsByStore.RenderOrderNotes)
            {
                var orderNotes = (await _orderService.GetOrderNotes(order.Id))
                    .Where(on => on.DisplayToCustomer)
                    .OrderByDescending(on => on.CreatedOnUtc)
                    .ToList();
                if (orderNotes.Any())
                {
                    var notesHeader = new PdfPTable(1);
                    notesHeader.RunDirection = GetDirection(lang);
                    notesHeader.WidthPercentage = 100f;
                    var cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes", lang.Id), PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName)));
                    cellOrderNote.Border = Rectangle.NO_BORDER;
                    notesHeader.AddCell(cellOrderNote);
                    doc.Add(notesHeader);
                    doc.Add(new Paragraph(" "));
                    var notesTable = PrepareOrderNotes(order, orderNotes, lang);
                    doc.Add(notesTable);
                }
            }

            #endregion

            #region Footer

            if (!string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) || !string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2))
            {
                PrepareOrderFooter(lang, pdfSettingsByStore, pdfWriter, pageSize);
            }

            #endregion
        }

        protected virtual async Task<PdfPTable> PrepareOrderHeader(Order order, Language language, PdfSettings pdfSettings)
        {

            //logo
            var logoPicture = await _pictureService.GetPictureById(pdfSettings.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1) {
                RunDirection = GetDirection(language)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            //store info
            var store = await _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            var anchor = new Anchor(store.Url.Trim(new[] { '/' }), font) {
                Reference = store.Url
            };

            var cellHeader = new PdfPCell(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Order#", language.Id), order.OrderNumber), titleFont));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(anchor));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.OrderDate", language.Id), _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(language.LanguageCulture))), font));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (logoExists)
                if (language.Rtl)
                    headerTable.SetWidths(new[] { 0.2f, 0.8f });
                else
                    headerTable.SetWidths(new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //logo               
            if (logoExists)
            {
                var logoFilePath = await _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(language, true);
                logo.ScaleToFit(65f, 65f);

                var cellLogo = new PdfPCell {
                    Border = Rectangle.NO_BORDER
                };
                cellLogo.AddElement(logo);
                headerTable.AddCell(cellLogo);
            }
            return headerTable;
        }

        protected virtual async Task<PdfPTable> PrepareOrderAddressHeader(Order order, Language language, PdfSettings pdfSettings)
        {
            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            var addressTable = new PdfPTable(2);
            addressTable.RunDirection = GetDirection(language);
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            //billing info
            var billingAddress = new PdfPTable(1);
            billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            billingAddress.RunDirection = GetDirection(language);

            billingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.BillingInformation", language.Id), titleFont));

            if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.BillingAddress.Company))
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Company", language.Id), order.BillingAddress.Company), font));

            billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Name", language.Id), order.BillingAddress.FirstName + " " + order.BillingAddress.LastName), font));
            if (_addressSettings.PhoneEnabled)
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Phone", language.Id), order.BillingAddress.PhoneNumber), font));
            if (_addressSettings.FaxEnabled && !String.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Fax", language.Id), order.BillingAddress.FaxNumber), font));
            if (_addressSettings.StreetAddressEnabled)
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address", language.Id), order.BillingAddress.Address1), font));
            if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.BillingAddress.Address2))
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address2", language.Id), order.BillingAddress.Address2), font));
            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var state = await _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(order.BillingAddress.StateProvinceId);
                billingAddress.AddCell(new Paragraph("   " + String.Format("{0}, {1} {2}", order.BillingAddress.City, !String.IsNullOrEmpty(order.BillingAddress.StateProvinceId) ? state.GetLocalized(x => x.Name, language.Id) : "", order.BillingAddress.ZipPostalCode), font));
            }
            if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(order.BillingAddress.CountryId))
            {
                var country = await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.BillingAddress.CountryId);
                billingAddress.AddCell(new Paragraph("   " + String.Format("{0}", !String.IsNullOrEmpty(order.BillingAddress.CountryId) ? country.GetLocalized(x => x.Name, language.Id) : ""), font));
            }
            //VAT number
            if (!String.IsNullOrEmpty(order.VatNumber))
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.VATNumber", language.Id), order.VatNumber), font));

            //custom attributes
            var customBillingAddressAttributes = await _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            if (!String.IsNullOrEmpty(customBillingAddressAttributes))
            {
                //TODO: we should add padding to each line (in case if we have sevaral custom address attributes)
                billingAddress.AddCell(new Paragraph("   " + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
            }


            //payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            string paymentMethodStr = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, language.Id) : order.PaymentMethodSystemName;
            if (!String.IsNullOrEmpty(paymentMethodStr))
            {
                billingAddress.AddCell(new Paragraph(" "));
                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.PaymentMethod", language.Id), paymentMethodStr), font));
                billingAddress.AddCell(new Paragraph());
            }

            //custom values
            var customValues = order.DeserializeCustomValues();
            if (customValues != null)
            {
                foreach (var item in customValues)
                {
                    billingAddress.AddCell(new Paragraph(" "));
                    billingAddress.AddCell(new Paragraph("   " + item.Key + ": " + item.Value, font));
                    billingAddress.AddCell(new Paragraph());
                }
            }

            addressTable.AddCell(billingAddress);

            //shipping info
            var shippingAddress = new PdfPTable(1);
            shippingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            shippingAddress.RunDirection = GetDirection(language);

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {

                if (!order.PickUpInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new GrandException(string.Format("Shipping is required, but address is not available. Order ID = {0}", order.Id));

                    shippingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.ShippingInformation", language.Id), titleFont));
                    if (!String.IsNullOrEmpty(order.ShippingAddress.Company))
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Company", language.Id), order.ShippingAddress.Company), font));
                    shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Name", language.Id), order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName), font));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Phone", language.Id), order.ShippingAddress.PhoneNumber), font));
                    if (_addressSettings.FaxEnabled && !String.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Fax", language.Id), order.ShippingAddress.FaxNumber), font));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address", language.Id), order.ShippingAddress.Address1), font));
                    if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.ShippingAddress.Address2))
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address2", language.Id), order.ShippingAddress.Address2), font));
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var state = await _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(order.ShippingAddress.StateProvinceId);
                        shippingAddress.AddCell(new Paragraph("   " + String.Format("{0}, {1} {2}", order.ShippingAddress.City, !String.IsNullOrEmpty(order.ShippingAddress.StateProvinceId) ? state.GetLocalized(x => x.Name, language.Id) : "", order.ShippingAddress.ZipPostalCode), font));
                    }
                    if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(order.ShippingAddress.CountryId))
                    {
                        var country = await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.ShippingAddress.CountryId);
                        shippingAddress.AddCell(new Paragraph("   " + String.Format("{0}", !String.IsNullOrEmpty(order.ShippingAddress.CountryId) ? country.GetLocalized(x => x.Name, language.Id) : ""), font));
                    }
                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!String.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        //TODO: we should add padding to each line (in case if we have sevaral custom address attributes)
                        shippingAddress.AddCell(new Paragraph("   " + HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                    shippingAddress.AddCell(new Paragraph(" "));
                }
                else
                    if (order.PickupPoint != null)
                {
                    if (order.PickupPoint.Address != null)
                    {
                        shippingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Pickup", language.Id), titleFont));
                        if (!string.IsNullOrEmpty(order.PickupPoint.Address.Address1))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", string.Format(_localizationService.GetResource("PDFInvoice.Address", language.Id), order.PickupPoint.Address.Address1)), font));
                        if (!string.IsNullOrEmpty(order.PickupPoint.Address.City))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", order.PickupPoint.Address.City), font));
                        if (!string.IsNullOrEmpty(order.PickupPoint.Address.CountryId))
                        {
                            var country = await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.PickupPoint.Address.CountryId);
                            if (country != null)
                                shippingAddress.AddCell(new Paragraph(string.Format("   {0}", country.Name), font));
                        }
                        if (!string.IsNullOrEmpty(order.PickupPoint.Address.ZipPostalCode))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", order.PickupPoint.Address.ZipPostalCode), font));

                        shippingAddress.AddCell(new Paragraph(" "));
                    }
                }

                shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.ShippingMethod", language.Id), order.ShippingMethod), font));
                shippingAddress.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddress);
            }
            else
            {
                shippingAddress.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddress);
            }
            return addressTable;
        }

        protected virtual async Task<PdfPTable> PrepareOrderProducts(Order order, Language language, string vendorId = "")
        {
            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            //products
            var productsTable = new PdfPTable(_catalogSettings.ShowSkuOnProductDetailsPage ? 5 : 4);
            productsTable.RunDirection = GetDirection(language);
            productsTable.WidthPercentage = 100f;
            if (language.Rtl)
            {
                productsTable.SetWidths(_catalogSettings.ShowSkuOnProductDetailsPage
                    ? new[] { 15, 10, 15, 15, 45 }
                    : new[] { 20, 10, 20, 50 });
            }
            else
            {
                productsTable.SetWidths(_catalogSettings.ShowSkuOnProductDetailsPage
                    ? new[] { 45, 15, 15, 10, 15 }
                    : new[] { 50, 20, 10, 20 });
            }

            //product name
            var cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductName", language.Id), font));
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.SKU", language.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductPrice", language.Id), font));
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductQuantity", language.Id), font));
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductTotal", language.Id), font));
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            foreach (var orderItem in order.OrderItems)
            {
                var pAttribTable = new PdfPTable(1);
                pAttribTable.RunDirection = GetDirection(language);
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                //a vendor should have access only to his products
                if (!String.IsNullOrEmpty(vendorId) && product.VendorId != vendorId)
                    continue;

                //product name
                string name = product.GetLocalized(x => x.Name, language.Id);
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesFont = PdfExtensions.GetFont(_pdfSettings.FontFileName);
                    attributesFont.SetStyle(Font.ITALIC);

                    var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    cellProductItem = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice = string.Empty;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                cellProductItem = new PdfPCell(new Phrase(unitPrice, font));
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = new PdfPCell(new Phrase(orderItem.Quantity.ToString(), font));
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal = string.Empty;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                cellProductItem = new PdfPCell(new Phrase(subTotal, font));
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }
            return productsTable;
        }

        protected virtual PdfPTable PrepareOrderCheckoutattributes(Order order, Language language)
        {
            var attribTable = new PdfPTable(1);
            attribTable.RunDirection = GetDirection(language);
            attribTable.WidthPercentage = 100f;

            string attributes = HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true);
            var cCheckoutAttributes = new PdfPCell(new Phrase(attributes, PdfExtensions.GetFont(_pdfSettings.FontFileName)));
            cCheckoutAttributes.Border = Rectangle.NO_BORDER;
            cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_RIGHT;
            attribTable.AddCell(cCheckoutAttributes);

            return attribTable;
        }

        protected virtual async Task<PdfPTable> PrepareOrderTotal(Order order, Language language)
        {
            //subtotal
            var totalsTable = new PdfPTable(1);
            totalsTable.RunDirection = GetDirection(language);
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            totalsTable.WidthPercentage = 100f;

            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                string orderSubtotalInclTaxStr = await _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);

                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", language.Id), orderSubtotalInclTaxStr), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                string orderSubtotalExclTaxStr = await _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);

                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", language.Id), orderSubtotalExclTaxStr), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    string orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", language.Id), orderSubTotalDiscountInCustomerCurrencyStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    string orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", language.Id), orderSubTotalDiscountInCustomerCurrencyStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    string orderShippingInclTaxStr = await _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", language.Id), orderShippingInclTaxStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    string orderShippingExclTaxStr = await _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", language.Id), orderShippingExclTaxStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    string paymentMethodAdditionalFeeInclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", language.Id), paymentMethodAdditionalFeeInclTaxStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    string paymentMethodAdditionalFeeExclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", language.Id), paymentMethodAdditionalFeeExclTaxStr), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            string taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = order.TaxRatesDictionary;

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = await _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                }
            }
            if (displayTax)
            {
                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Tax", language.Id), taxStr), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    string taxRate = String.Format(_localizationService.GetResource("PDFInvoice.TaxRate", language.Id), _priceFormatter.FormatTaxRate(item.Key));
                    string taxValue = await _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, language);

                    var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", taxRate, taxValue), font));
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                string orderDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);

                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", language.Id), orderDiscountInCustomerCurrencyStr), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            var _serviceGiftCard = _serviceProvider.GetRequiredService<IGiftCardService>();
            //gift cards
            foreach (var gcuh in await _serviceGiftCard.GetAllGiftCardUsageHistory(order.Id))
            {
                var giftcard = await _serviceProvider.GetRequiredService<IGiftCardService>().GetGiftCardById(gcuh.GiftCardId);
                string gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", language.Id), giftcard.GiftCardCouponCode);
                string gcAmountStr = await _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);

                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", gcTitle, gcAmountStr), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                string rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", language.Id), -order.RedeemedRewardPointsEntry.Points);
                string rpAmount = await _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);

                var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", rpTitle, rpAmount), font));
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            string orderTotalStr = await _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);

            var pTotal = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.OrderTotal", language.Id), orderTotalStr), PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName)));
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);

            return totalsTable;

        }

        protected virtual PdfPTable PrepareOrderNotes(Order order, List<OrderNote> orderNotes, Language language)
        {
            var notesTable = new PdfPTable(2);
            notesTable.RunDirection = GetDirection(language);
            if (language.Rtl)
            {
                notesTable.SetWidths(new[] { 70, 30 });
            }
            else
            {
                notesTable.SetWidths(new[] { 30, 70 });
            }
            notesTable.WidthPercentage = 100f;

            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            var cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes", language.Id), titleFont));
            cellOrderNote.Border = Rectangle.NO_BORDER;

            //created on
            cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.CreatedOn", language.Id), font));
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            //note
            cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.Note", language.Id), font));
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            foreach (var orderNote in orderNotes)
            {
                cellOrderNote = new PdfPCell(new Phrase(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc).ToString(), font));
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                cellOrderNote = new PdfPCell(new Phrase(HtmlHelper.ConvertHtmlToPlainText(orderNote.FormatOrderNoteText(), true, true), font));
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                //should we display a link to downloadable files here?
                //I think, no. Onyway, PDFs are printable documents and links (files) are useful here
            }

            return notesTable;
        }

        protected virtual void PrepareOrderFooter(Language language, PdfSettings pdfSettings, PdfWriter pdfWriter, Rectangle pageSize)
        {
            var column1Lines = String.IsNullOrEmpty(pdfSettings.InvoiceFooterTextColumn1) ?
                        new List<string>() :
                        pdfSettings.InvoiceFooterTextColumn1
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
            var column2Lines = String.IsNullOrEmpty(pdfSettings.InvoiceFooterTextColumn2) ?
                new List<string>() :
                pdfSettings.InvoiceFooterTextColumn2
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (column1Lines.Any() || column2Lines.Any())
            {
                var totalLines = Math.Max(column1Lines.Count, column2Lines.Count);
                const float margin = 43;

                //if you have really a lot of lines in the footer, then replace 9 with 10 or 11
                int footerHeight = totalLines * 9;
                var directContent = pdfWriter.DirectContent;
                directContent.MoveTo(pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight);
                directContent.LineTo(pageSize.GetRight(margin), pageSize.GetBottom(margin) + footerHeight);
                directContent.Stroke();


                var footerTable = new PdfPTable(2);
                footerTable.WidthPercentage = 100f;
                footerTable.SetTotalWidth(new float[] { 250, 250 });
                footerTable.RunDirection = GetDirection(language);

                var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);

                //column 1
                if (column1Lines.Any())
                {
                    var column1 = new PdfPCell(new Phrase());
                    column1.Border = Rectangle.NO_BORDER;
                    column1.HorizontalAlignment = Element.ALIGN_LEFT;
                    foreach (var footerLine in column1Lines)
                    {
                        column1.Phrase.Add(new Phrase(footerLine, font));
                        column1.Phrase.Add(new Phrase(Environment.NewLine));
                    }
                    footerTable.AddCell(column1);
                }
                else
                {
                    var column = new PdfPCell(new Phrase(" "));
                    column.Border = Rectangle.NO_BORDER;
                    footerTable.AddCell(column);
                }

                //column 2
                if (column2Lines.Any())
                {
                    var column2 = new PdfPCell(new Phrase());
                    column2.Border = Rectangle.NO_BORDER;
                    column2.HorizontalAlignment = Element.ALIGN_LEFT;
                    foreach (var footerLine in column2Lines)
                    {
                        column2.Phrase.Add(new Phrase(footerLine, font));
                        column2.Phrase.Add(new Phrase(Environment.NewLine));
                    }
                    footerTable.AddCell(column2);
                }
                else
                {
                    var column = new PdfPCell(new Phrase(" "));
                    column.Border = Rectangle.NO_BORDER;
                    footerTable.AddCell(column);
                }

                footerTable.WriteSelectedRows(0, totalLines, pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight, directContent);
            }
        }


        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }
        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

            return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }
    }
}
