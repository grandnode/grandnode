using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Localization;
using Grand.Services.Media;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IMediator _mediator;
        private readonly PdfSettings _pdfSettings;

        #endregion

        #region Ctor

        public PdfService(
            ILanguageService languageService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IMediator mediator,
            PdfSettings pdfSettings)
        {
            _languageService = languageService;
            _workContext = workContext;
            _downloadService = downloadService;
            _mediator = mediator;
            _pdfSettings = pdfSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>A path of generated file</returns>
        public virtual async Task<string> PrintOrderToPdf(Order order, string languageId, string vendorId = "")
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

        /// <summary>
        /// Save an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor ident</param>
        /// <returns>A download ident</returns>
        public virtual async Task<string> SaveOrderToBinary(Order order, string languageId, string vendorId = "")
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

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual async Task PrintOrdersToPdf(Stream stream, IList<Order> orders, string languageId = "", string vendorId = "")
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (orders == null)
                throw new ArgumentNullException("orders");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                await _mediator.Send(new PreparePdfOrderCommand() {
                    Doc = doc,
                    PdfWriter = pdfWriter,
                    PageSize = pageSize,
                    Order = order,
                    LanguageId = languageId,
                    VendorId = vendorId
                });
                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual async Task PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, string languageId = "")
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (shipments == null)
                throw new ArgumentNullException("shipments");

            var lang = await _languageService.GetLanguageById(languageId);
            if (lang == null)
                throw new ArgumentException(string.Format("Cannot load language. ID={0}", languageId));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            int shipmentCount = shipments.Count;
            int shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                await _mediator.Send(new PreparePdfShipmentCommand() {
                    Doc = doc,
                    Language = lang,
                    LanguageId = languageId,
                    Shipment = shipment
                });

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }

        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual async Task PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (products == null)
                throw new ArgumentNullException("products");

            var lang = _workContext.WorkingLanguage;

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            int productNumber = 1;
            int prodCount = products.Count;

            foreach (var product in products)
            {
                var productTable = await _mediator.Send(new PreparePdfProductCommand() {
                    Product = product,
                    Language = lang,
                    ProductNumber = productNumber
                });

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}