using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Media
{
    /// <summary>
    /// Download service
    /// </summary>
    public partial class DownloadService : IDownloadService
    {
        #region Fields

        private readonly IRepository<Download> _downloadRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="downloadRepository">Download repository</param>
        /// <param name="mediator">Mediator</param>
        public DownloadService(IRepository<Download> downloadRepository,
            IMediator mediator)
        {
            _downloadRepository = downloadRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a download
        /// </summary>
        /// <param name="downloadId">Download identifier</param>
        /// <returns>Download</returns>
        public virtual async Task<Download> GetDownloadById(string downloadId)
        {
            if (string.IsNullOrEmpty(downloadId))
                return null;

            var _download = await _downloadRepository.GetByIdAsync(downloadId);
            if (!_download.UseDownloadUrl)
                _download.DownloadBinary = await DownloadAsBytes(_download.DownloadObjectId);

            return _download;
        }

        protected virtual async Task<byte[]> DownloadAsBytes(ObjectId objectId)
        {
            var bucket = new MongoDB.Driver.GridFS.GridFSBucket(_downloadRepository.Database);
            var binary = await bucket.DownloadAsBytesAsync(objectId, new MongoDB.Driver.GridFS.GridFSDownloadOptions() { CheckMD5 = true, Seekable = true });
            return binary;
        }
        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        /// <returns>Download</returns>
        public virtual async Task<Download> GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty)
                return null;

            var query = from o in _downloadRepository.Table
                        where o.DownloadGuid == downloadGuid
                        select o;
            var order = await query.FirstOrDefaultAsync();
            if (!order.UseDownloadUrl)
                order.DownloadBinary = await DownloadAsBytes(order.DownloadObjectId);

            return order;
        }

        /// <summary>
        /// Deletes a download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual async Task DeleteDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            await _downloadRepository.DeleteAsync(download);

            await _mediator.EntityDeleted(download);
        }

        /// <summary>
        /// Inserts a download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual async Task InsertDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");
            if (!download.UseDownloadUrl)
            {
                var bucket = new MongoDB.Driver.GridFS.GridFSBucket(_downloadRepository.Database);
                var id = await bucket.UploadFromBytesAsync(download.Filename, download.DownloadBinary);
                download.DownloadObjectId = id;
            }

            download.DownloadBinary = null;
            await _downloadRepository.InsertAsync(download);

            await _mediator.EntityInserted(download);
        }

        /// <summary>
        /// Updates the download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual async Task UpdateDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            await _downloadRepository.UpdateAsync(download);

            await _mediator.EntityUpdated(download);
        }

        /// <summary>
        /// Gets a value indicating whether download is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItem">Order item to check</param>
        /// <param name="product">Product</param>
        /// <returns>True if download is allowed; otherwise, false.</returns>
        public virtual bool IsDownloadAllowed(Order order, OrderItem orderItem, Product product)
        {
            if (orderItem == null)
                return false;

            if (order == null || order.Deleted)
                return false;

            //order status
            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (product == null || !product.IsDownload)
                return false;

            //payment status
            switch (product.DownloadActivationType)
            {
                case DownloadActivationType.WhenOrderIsPaid:
                    {
                        if (order.PaymentStatus == PaymentStatus.Paid && order.PaidDateUtc.HasValue)
                        {
                            //expiration date
                            if (product.DownloadExpirationDays.HasValue)
                            {
                                if (order.PaidDateUtc.Value.AddDays(product.DownloadExpirationDays.Value) > DateTime.UtcNow)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case DownloadActivationType.Manually:
                    {
                        if (orderItem.IsDownloadActivated)
                        {
                            //expiration date
                            if (product.DownloadExpirationDays.HasValue)
                            {
                                if (order.CreatedOnUtc.AddDays(product.DownloadExpirationDays.Value) > DateTime.UtcNow)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether license download is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItem">Order item to check</param>
        /// <param name="product">Product</param>
        /// <returns>True if license download is allowed; otherwise, false.</returns>
        public virtual bool IsLicenseDownloadAllowed(Order order, OrderItem orderItem, Product product)
        {
            if (orderItem == null)
                return false;

            return !string.IsNullOrEmpty(orderItem.LicenseDownloadId) && IsDownloadAllowed(order, orderItem, product);
        }

        #endregion
    }
}
