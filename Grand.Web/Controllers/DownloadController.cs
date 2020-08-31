using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Documents;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class DownloadController : BasePublicController
    {
        private readonly IDownloadService _downloadService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly CustomerSettings _customerSettings;

        public DownloadController(IDownloadService downloadService,
            IProductService productService,
            IOrderService orderService,
            IReturnRequestService returnRequestService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            CustomerSettings customerSettings)
        {
            _downloadService = downloadService;
            _productService = productService;
            _orderService = orderService;
            _returnRequestService = returnRequestService;
            _workContext = workContext;
            _localizationService = localizationService;
            _customerSettings = customerSettings;
        }

        public virtual async Task<IActionResult> Sample(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                return InvokeHttp404();

            if (!product.HasSampleDownload)
                return Content("Product doesn't have a sample download.");

            var download = await _downloadService.GetDownloadById(product.SampleDownloadId);
            if (download == null)
                return Content("Sample download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetDownload(Guid orderItemId, bool agree = false)
        {
            var orderItem = await _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return InvokeHttp404();

            var order = await _orderService.GetOrderByOrderItemId(orderItem.Id);
            var product = await _productService.GetProductById(orderItem.ProductId);
            if (!_downloadService.IsDownloadAllowed(order, orderItem, product))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return Challenge();

                if (order.CustomerId != _workContext.CurrentCustomer.Id && order.OwnerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var download = await _downloadService.GetDownloadById(product.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (product.HasUserAgreement)
            {
                if (!agree)
                    return RedirectToRoute("DownloadUserAgreement", new { orderItemId = orderItemId });
            }


            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return Content(string.Format(_localizationService.GetResource("DownloadableProducts.ReachedMaximumNumber"), product.MaxNumberOfDownloads));


            if (download.UseDownloadUrl)
            {
                //increase download
                order.OrderItems.FirstOrDefault(x => x.Id == orderItem.Id).DownloadCount++;
                await _orderService.UpdateOrder(order);

                //return result
                return new RedirectResult(download.DownloadUrl);
            }

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //increase download
            order.OrderItems.FirstOrDefault(x => x.Id == orderItem.Id).DownloadCount++;
            await _orderService.UpdateOrder(order);

            if (product.ProductType != ProductType.BundledProduct)
            {
                //return result
                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        string fileName = (!String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString()) + download.Extension;
                        if (Grand.Core.OperatingSystem.IsWindows())
                        {
                            System.IO.File.WriteAllBytes(@"App_Data\Download\" + fileName, download.DownloadBinary);
                            ziparchive.CreateEntryFromFile(@"App_Data\Download\" + fileName, fileName);
                        }
                        else
                        {
                            System.IO.File.WriteAllBytes(@"App_Data/Download/" + fileName, download.DownloadBinary);
                            ziparchive.CreateEntryFromFile(@"App_Data/Download/" + fileName, fileName);
                        }
                        foreach (var bundle in product.BundleProducts)
                        {
                            var p1 = await _productService.GetProductById(bundle.ProductId);
                            if (p1 != null && p1.IsDownload)
                            {
                                var d1 = await _downloadService.GetDownloadById(p1.DownloadId);
                                if (d1 != null && !d1.UseDownloadUrl)
                                {
                                    fileName = (!String.IsNullOrWhiteSpace(d1.Filename) ? d1.Filename : p1.Id.ToString()) + d1.Extension;
                                    if (Grand.Core.OperatingSystem.IsWindows())
                                    {
                                        System.IO.File.WriteAllBytes(@"App_Data\Download\" + fileName, d1.DownloadBinary);
                                        ziparchive.CreateEntryFromFile(@"App_Data\Download\" + fileName, fileName);
                                    }
                                    else
                                    {
                                        System.IO.File.WriteAllBytes(@"App_Data/Download/" + fileName, d1.DownloadBinary);
                                        ziparchive.CreateEntryFromFile(@"App_Data/Download/" + fileName, fileName);
                                    }
                                }
                            }
                        }
                    }
                    return File(memoryStream.ToArray(), "application/zip", $"{Regex.Replace(product.Name, "[^A-Za-z0-9 _]", "")}.zip");
                }
            }
        }

        public virtual async Task<IActionResult> GetLicense(Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return InvokeHttp404();

            var order = await _orderService.GetOrderByOrderItemId(orderItem.Id);
            var product = await _productService.GetProductById(orderItem.ProductId);
            if (!_downloadService.IsLicenseDownloadAllowed(order, orderItem, product))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (order.CustomerId != _workContext.CurrentCustomer.Id && order.OwnerId != _workContext.CurrentCustomer.Id)
                    return Challenge();
            }

            var download = await _downloadService.GetDownloadById(!String.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "");
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetFileUpload(Guid downloadId)
        {
            var download = await _downloadService.GetDownloadByGuid(downloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetOrderNoteFile(string orderNoteId)
        {
            var orderNote = await _orderService.GetOrderNote(orderNoteId);
            if (orderNote == null)
                return InvokeHttp404();

            var order = await _orderService.GetOrderById(orderNote.OrderId);
            if (order == null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || (order.CustomerId != _workContext.CurrentCustomer.Id && order.OwnerId != _workContext.CurrentCustomer.Id))
                return Challenge();

            var download = await _downloadService.GetDownloadById(orderNote.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetShipmentNoteFile(string shipmentNoteId, 
            [FromServices] IShipmentService shipmentService)
        {
            var shipmentNote = await shipmentService.GetShipmentNote(shipmentNoteId);
            if (shipmentNote == null)
                return InvokeHttp404();

            var shipment = await shipmentService.GetShipmentById(shipmentNote.ShipmentId);
            if (shipment == null)
                return InvokeHttp404();

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || (order.CustomerId != _workContext.CurrentCustomer.Id && order.OwnerId != _workContext.CurrentCustomer.Id))
                return Challenge();

            var download = await _downloadService.GetDownloadById(shipmentNote.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : shipmentNote.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetCustomerNoteFile(string customerNoteId,
            [FromServices] ICustomerService customerService)
        {
            if (string.IsNullOrEmpty(customerNoteId))
                return Content("Download is not available.");

            var customerNote = await customerService.GetCustomerNote(customerNoteId);
            if (customerNote == null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || customerNote.CustomerId != _workContext.CurrentCustomer.Id)
                return Challenge();

            var download = await _downloadService.GetDownloadById(customerNote.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : customerNote.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetReturnRequestNoteFile(string returnRequestNoteId)
        {
            var returnRequestNote = await _returnRequestService.GetReturnRequestNote(returnRequestNoteId);
            if (returnRequestNote == null)
                return InvokeHttp404();

            var returnRequest = await _returnRequestService.GetReturnRequestById(returnRequestNote.ReturnRequestId);
            if (returnRequest == null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || returnRequest.CustomerId != _workContext.CurrentCustomer.Id)
                return Challenge();

            var download = await _downloadService.GetDownloadById(returnRequestNote.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : returnRequestNote.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public virtual async Task<IActionResult> GetDocumentFile(string documentId,
            [FromServices] IDocumentService documentService)
        {
            if (string.IsNullOrEmpty(documentId))
                return Content("Download is not available.");

            var document = await documentService.GetById(documentId);
            if (document == null || !document.Published)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || document.CustomerId != _workContext.CurrentCustomer.Id)
                return Challenge();

            var download = await _downloadService.GetDownloadById(document.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : document.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }
    }
}
