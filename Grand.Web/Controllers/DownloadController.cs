using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Grand.Web.Controllers
{
    public partial class DownloadController : BasePublicController
    {
        private readonly IDownloadService _downloadService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly CustomerSettings _customerSettings;

        public DownloadController(IDownloadService downloadService,
            IProductService productService,
            IOrderService orderService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            CustomerSettings customerSettings)
        {
            this._downloadService = downloadService;
            this._productService = productService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._customerSettings = customerSettings;
        }
        
        public virtual IActionResult Sample(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return InvokeHttp404();
            
            if (!product.HasSampleDownload)
                return Content("Product doesn't have a sample download.");

            var download = _downloadService.GetDownloadById(product.SampleDownloadId);
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

        public virtual IActionResult GetDownload(Guid orderItemId, bool agree = false)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return InvokeHttp404();

            var order = _orderService.GetOrderByOrderItemId(orderItem.Id);
            var product = _productService.GetProductById(orderItem.ProductId);
            if (!_downloadService.IsDownloadAllowed(orderItem))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return Challenge();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var download = _downloadService.GetDownloadById(product.DownloadId);
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
                _orderService.UpdateOrder(order);

                //return result
                return new RedirectResult(download.DownloadUrl);
            }
            
            //binary download
            if (download.DownloadBinary == null)
                    return Content("Download data is not available any more.");

            //increase download
            order.OrderItems.FirstOrDefault(x => x.Id == orderItem.Id).DownloadCount++;
            _orderService.UpdateOrder(order);

            if(product.ProductType != ProductType.BundledProduct)
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
                            var p1 = _productService.GetProductById(bundle.ProductId);
                            if(p1!=null && p1.IsDownload)
                            {
                                var d1 = _downloadService.GetDownloadById(p1.DownloadId);
                                if(d1!=null && !d1.UseDownloadUrl)
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

        public virtual IActionResult GetLicense(Guid orderItemId)
        {
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return InvokeHttp404();

            var order = _orderService.GetOrderByOrderItemId(orderItem.Id);
            var product = _productService.GetProductById(orderItem.ProductId);
            if (!_downloadService.IsLicenseDownloadAllowed(orderItem))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null || order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Challenge();
            }

            var download = _downloadService.GetDownloadById(!String.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "");
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

        public virtual IActionResult GetFileUpload(Guid downloadId)
        {
            var download = _downloadService.GetDownloadByGuid(downloadId);
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

        public virtual IActionResult GetOrderNoteFile(string orderNoteId)
        {
            var orderNote = _orderService.GetOrderNote(orderNoteId);
            if (orderNote == null)
                return InvokeHttp404();

            var order = _orderService.GetOrderById(orderNote.OrderId);
            if (order == null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || order.CustomerId != _workContext.CurrentCustomer.Id)
                return Challenge();

            var download = _downloadService.GetDownloadById(orderNote.DownloadId);
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

        public virtual IActionResult GetCustomerNoteFile(string customerNoteId, 
            [FromServices] ICustomerService customerService)
        {
            if(string.IsNullOrEmpty(customerNoteId))
                return Content("Download is not available.");

            var customerNote = customerService.GetCustomerNote(customerNoteId);
            if (customerNote==null)
                return InvokeHttp404();

            if (_workContext.CurrentCustomer == null || customerNote.CustomerId != _workContext.CurrentCustomer.Id)
                return Challenge();

            var download = _downloadService.GetDownloadById(customerNote.DownloadId);
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


    }
}
