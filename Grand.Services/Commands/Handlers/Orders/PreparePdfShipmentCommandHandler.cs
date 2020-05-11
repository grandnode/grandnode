using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Html;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class PreparePdfShipmentCommandHandler : IRequestHandler<PreparePdfShipmentCommand, bool>
    {
        private readonly IOrderService _orderService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IServiceProvider _serviceProvider;
        private readonly PdfSettings _pdfSettings;
        private readonly AddressSettings _addressSettings;

        public PreparePdfShipmentCommandHandler(
            IOrderService orderService,
            ILanguageService languageService,
            IWorkContext workContext,
            IProductService productService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            IServiceProvider serviceProvider,
            PdfSettings pdfSettings,
            AddressSettings addressSettings)
        {
            _orderService = orderService;
            _languageService = languageService;
            _workContext = workContext;
            _productService = productService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _addressAttributeFormatter = addressAttributeFormatter;
            _serviceProvider = serviceProvider;
            _pdfSettings = pdfSettings;
            _addressSettings = addressSettings;
        }

        public async Task<bool> Handle(PreparePdfShipmentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderById(request.Shipment.OrderId);
            var lang = request.Language;

            if (string.IsNullOrEmpty(request.LanguageId))
            {
                request.Language = await _languageService.GetLanguageById(order.CustomerLanguageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;
            }
            var addressTable = await PreparePackagingSlipsAddress(order, request.Shipment, lang);
            request.Doc.Add(addressTable);

            var productsTable = await PreparePackagingSlipsProducts(order, request.Shipment, lang);
            request.Doc.Add(productsTable);

            return true;
        }

        protected virtual async Task<PdfPTable> PreparePackagingSlipsAddress(Order order, Shipment shipment, Language language)
        {
            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            var addressTable = new PdfPTable(1);
            if (language.Rtl)
                addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;

            addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Shipment", language.Id), shipment.ShipmentNumber), titleFont));
            addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Order", language.Id), order.OrderNumber), titleFont));

            if (!order.PickUpInStore)
            {
                if (order.ShippingAddress == null)
                    throw new GrandException(string.Format("Shipping is required, but address is not available. Order ID = {0}", order.Id));

                if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.ShippingAddress.Company))
                    addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Company", language.Id),
                                order.ShippingAddress.Company), font));

                addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Name", language.Id),
                            order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName), font));
                if (_addressSettings.PhoneEnabled)
                    addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Phone", language.Id),
                                order.ShippingAddress.PhoneNumber), font));
                if (_addressSettings.StreetAddressEnabled)
                    addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address", language.Id),
                                order.ShippingAddress.Address1), font));

                if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.ShippingAddress.Address2))
                    addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address2", language.Id),
                                order.ShippingAddress.Address2), font));

                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                    addressTable.AddCell(new Paragraph(String.Format("{0}, {1} {2}", order.ShippingAddress.City, !String.IsNullOrEmpty(order.ShippingAddress.StateProvinceId)
                                    ? (await _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(order.ShippingAddress.StateProvinceId)).GetLocalized(x => x.Name, language.Id)
                                    : "", order.ShippingAddress.ZipPostalCode), font));

                if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(order.ShippingAddress.CountryId))
                    addressTable.AddCell(new Paragraph(String.Format("{0}", !String.IsNullOrEmpty(order.ShippingAddress.CountryId)
                                    ? (await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.ShippingAddress.CountryId)).GetLocalized(x => x.Name, language.Id)
                                    : ""), font));

                //custom attributes
                var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                if (!String.IsNullOrEmpty(customShippingAddressAttributes))
                {
                    addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                }
            }
            else
                if (order.PickupPoint != null)
            {
                if (order.PickupPoint.Address != null)
                {
                    addressTable.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Pickup", language.Id), titleFont));
                    if (!string.IsNullOrEmpty(order.PickupPoint.Address.Address1))
                        addressTable.AddCell(new Paragraph(string.Format("   {0}", string.Format(_localizationService.GetResource("PDFInvoice.Address", language.Id), order.PickupPoint.Address.Address1)), font));
                    if (!string.IsNullOrEmpty(order.PickupPoint.Address.City))
                        addressTable.AddCell(new Paragraph(string.Format("   {0}", order.PickupPoint.Address.City), font));
                    if (!string.IsNullOrEmpty(order.PickupPoint.Address.CountryId))
                    {
                        var country = await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(order.PickupPoint.Address.CountryId);
                        if (country != null)
                            addressTable.AddCell(new Paragraph(string.Format("   {0}", country.Name), font));
                    }
                    if (!string.IsNullOrEmpty(order.PickupPoint.Address.ZipPostalCode))
                        addressTable.AddCell(new Paragraph(string.Format("   {0}", order.PickupPoint.Address.ZipPostalCode), font));
                    addressTable.AddCell(new Paragraph(" "));
                }
            }

            addressTable.AddCell(new Paragraph(" "));

            addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.ShippingMethod", language.Id), order.ShippingMethod), font));
            addressTable.AddCell(new Paragraph(" "));

            return addressTable;
        }

        protected virtual async Task<PdfPTable> PreparePackagingSlipsProducts(Order order, Shipment shipment, Language language)
        {
            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);

            var productsTable = new PdfPTable(3);
            productsTable.WidthPercentage = 100f;
            if (language.Rtl)
            {
                productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                productsTable.SetWidths(new[] { 20, 20, 60 });
            }
            else
            {
                productsTable.SetWidths(new[] { 60, 20, 20 });
            }

            //product name
            var cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.ProductName", language.Id), font));
            cell.BackgroundColor = BaseColor.LightGray;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cell);

            //SKU
            cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.SKU", language.Id), font));
            cell.BackgroundColor = BaseColor.LightGray;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cell);

            //qty
            cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.QTY", language.Id), font));
            cell.BackgroundColor = BaseColor.LightGray;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cell);

            foreach (var si in shipment.ShipmentItems)
            {
                var productAttribTable = new PdfPTable(1);
                if (language.Rtl)
                    productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                var orderItem = order.OrderItems.Where(x => x.Id == si.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                string name = product.GetLocalized(x => x.Name, language.Id);
                productAttribTable.AddCell(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesFont = PdfExtensions.GetFont(_pdfSettings.FontFileName);
                    attributesFont.SetStyle(Font.ITALIC);

                    var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                    productAttribTable.AddCell(attributesParagraph);
                }

                productsTable.AddCell(productAttribTable);

                //SKU
                var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                cell = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = new PdfPCell(new Phrase(si.Quantity.ToString(), font));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);
            }

            return productsTable;
        }

    }
}
