using System;
using System.Linq;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Services.Events;
using Grand.Core.Infrastructure;
using Grand.Services.Orders;
using MongoDB.Bson;
using Grand.Services.Catalog;

namespace Grand.Services.Media
{
    /// <summary>
    /// Download service
    /// </summary>
    public partial class DownloadService : IDownloadService
    {
        #region Fields

        private readonly IRepository<Download> _downloadRepository;
        private readonly IEventPublisher _eventPubisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="downloadRepository">Download repository</param>
        /// <param name="eventPubisher"></param>
        public DownloadService(IRepository<Download> downloadRepository,
            IEventPublisher eventPubisher)
        {
            _downloadRepository = downloadRepository;
            _eventPubisher = eventPubisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a download
        /// </summary>
        /// <param name="downloadId">Download identifier</param>
        /// <returns>Download</returns>
        public virtual Download GetDownloadById(string downloadId)
        {
            if (String.IsNullOrEmpty(downloadId))
                return null;

            var _download = _downloadRepository.GetById(downloadId);
            if(!_download.UseDownloadUrl)
                _download.DownloadBinary = DownloadAsBytes(_download.DownloadObjectId);

            return _download;
        }

        protected virtual byte[] DownloadAsBytes(ObjectId objectId)
        {
            var bucket = new MongoDB.Driver.GridFS.GridFSBucket(_downloadRepository.Database);
            var binary = bucket.DownloadAsBytesAsync(objectId, new MongoDB.Driver.GridFS.GridFSDownloadOptions() { CheckMD5 = true, Seekable = true }).Result;
            return binary;
        }
        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        /// <returns>Download</returns>
        public virtual Download GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty)
                return null;

            var query = from o in _downloadRepository.Table
                        where o.DownloadGuid == downloadGuid
                        select o;
            var order = query.FirstOrDefault();
            if(!order.UseDownloadUrl)
                order.DownloadBinary = DownloadAsBytes(order.DownloadObjectId); 

            return order;
        }

        /// <summary>
        /// Deletes a download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual void DeleteDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            _downloadRepository.Delete(download);

            _eventPubisher.EntityDeleted(download);
        }

        /// <summary>
        /// Inserts a download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual void InsertDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");
            if (!download.UseDownloadUrl)
            {
                var bucket = new MongoDB.Driver.GridFS.GridFSBucket(_downloadRepository.Database);
                var id = bucket.UploadFromBytesAsync(download.Filename, download.DownloadBinary).Result;
                download.DownloadObjectId = id;
            }

            download.DownloadBinary = null;
            _downloadRepository.Insert(download);

            _eventPubisher.EntityInserted(download);
        }

        /// <summary>
        /// Updates the download
        /// </summary>
        /// <param name="download">Download</param>
        public virtual void UpdateDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            _downloadRepository.Update(download);

            _eventPubisher.EntityUpdated(download);
        }

        /// <summary>
        /// Gets a value indicating whether download is allowed
        /// </summary>
        /// <param name="orderItem">Order item to check</param>
        /// <returns>True if download is allowed; otherwise, false.</returns>
        public virtual bool IsDownloadAllowed(OrderItem orderItem)
        {
            if (orderItem == null)
                return false;

            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderByOrderItemId(orderItem.Id);
            if (order == null || order.Deleted)
                return false;

            //order status
            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(orderItem.ProductId);
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
        /// <param name="orderItem">Order item to check</param>
        /// <returns>True if license download is allowed; otherwise, false.</returns>
        public virtual bool IsLicenseDownloadAllowed(OrderItem orderItem)
        {
            if (orderItem == null)
                return false;

            return IsDownloadAllowed(orderItem) &&
                !String.IsNullOrEmpty(orderItem.LicenseDownloadId);
        }

        #endregion
    }
}
