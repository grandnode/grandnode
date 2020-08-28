using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Localization;
using Grand.Services.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generate invoice  products , shipment as pdf (from html template to pdf)
    /// </summary>
    public class WkPdfService : IPdfService
    {
        private const string _orderTemaplate = "~/Views/PdfTemplates/OrderPdfTemplate.cshtml";
        private const string _productsTemaplate = "~/Views/PdfTemplates/ProductsPdfTemplate.cshtml";
        private const string _shipmentsTemaplate = "~/Views/PdfTemplates/ShipmentPdfTemplate.cshtml";
        private const string _orderFooter = "pdf/footers/order.html";
        private const string _productsFooter = "pdf/footers/products.html";
        private const string _shipmentFooter = "pdf/footers/shipment.html";
        private readonly IGeneratePdf _generatePdf;
        private readonly IViewRenderService _viewRenderService;
        private readonly IDownloadService _downloadService;
        private readonly ILanguageService _languageService;

        public WkPdfService(IGeneratePdf generatePdf, IViewRenderService viewRenderService, IDownloadService downloadService,
            ILanguageService languageService)
        {
            _generatePdf = generatePdf;
            _viewRenderService = viewRenderService;
            _downloadService = downloadService;
            _languageService = languageService;
        }

        public async Task PrintOrdersToPdf(Stream stream, IList<Order> orders, string languageId = "", string vendorId = "")
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (orders == null)
                throw new ArgumentNullException("orders");

            _generatePdf.SetConvertOptions(new ConvertOptions() {
                PageSize = Wkhtmltopdf.NetCore.Options.Size.A4,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins() { Bottom = 10, Left = 10, Right = 10, Top = 10 },
                FooterHtml = CommonHelper.WebMapPath(_orderFooter)
            });

            var html = await _viewRenderService.RenderToStringAsync<IList<Order>>(_orderTemaplate, orders);
            var pdfBytes = _generatePdf.GetPDF(html);
            stream.Write(pdfBytes);
        }

        public async Task<string> PrintOrderToPdf(Order order, string languageId, string vendorId = "")
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var fileName = string.Format("order_{0}_{1}.pdf", order.OrderGuid, CommonHelper.GenerateRandomDigitCode(4));
            var filePath = Path.Combine(CommonHelper.WebMapPath("content/files/exportimport"), fileName);
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create);
                var orders = new List<Order>
                {
                    order
                };
                await PrintOrdersToPdf(fileStream, orders, languageId, vendorId);
            }
            finally
            {
                fileStream?.Dispose();
            }
            return filePath;
        }

        public async Task PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, string languageId = "")
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (shipments == null)
                throw new ArgumentNullException("shipments");

            var lang = await _languageService.GetLanguageById(languageId);
            if (lang == null)
                throw new ArgumentException(string.Format("Cannot load language. ID={0}", languageId));

            _generatePdf.SetConvertOptions(new ConvertOptions() {
                PageSize = Wkhtmltopdf.NetCore.Options.Size.A4,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins() { Bottom = 10, Left = 10, Right = 10, Top = 10 },
                FooterHtml = CommonHelper.WebMapPath(_shipmentFooter)
            });

            var html = await _viewRenderService.RenderToStringAsync<IList<Shipment>>(_shipmentsTemaplate, shipments);
            var pdfBytes = _generatePdf.GetPDF(html);
            stream.Write(pdfBytes);

        }

        public async Task PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (products == null)
                throw new ArgumentNullException("products");

            _generatePdf.SetConvertOptions(new ConvertOptions() {
                PageSize = Wkhtmltopdf.NetCore.Options.Size.A4,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins() { Bottom = 10, Left = 10, Right = 10, Top = 10 },
                FooterHtml = CommonHelper.WebMapPath(_productsFooter)
            });


            var html = await _viewRenderService.RenderToStringAsync<IList<Product>>(_productsTemaplate, products);
            var pdfBytes = _generatePdf.GetPDF(html);
            stream.Write(pdfBytes);
        }

        public async Task<string> SaveOrderToBinary(Order order, string languageId, string vendorId = "")
        {
            if (order == null)
                throw new ArgumentNullException("order");

            string fileName = string.Format("order_{0}_{1}", order.OrderGuid, CommonHelper.GenerateRandomDigitCode(4));
            string downloadId = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                var orders = new List<Order>
                {
                    order
                };
                await PrintOrdersToPdf(ms, orders, languageId, vendorId);
                var download = new Domain.Media.Download {
                    Filename = fileName,
                    Extension = ".pdf",
                    DownloadBinary = ms.ToArray(),
                    ContentType = "application/pdf",
                };
                await _downloadService.InsertDownload(download);
                downloadId = download.Id;
            }
            return downloadId;
        }
    }

}
