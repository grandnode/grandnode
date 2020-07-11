using Grand.Domain.Catalog;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Media
{
    /// <summary>
    /// Download service interface
    /// </summary>
    public partial interface IDownloadService
    {
        /// <summary>
        /// Gets a download
        /// </summary>
        /// <param name="downloadId">Download identifier</param>
        /// <returns>Download</returns>
        Task<Download> GetDownloadById(string downloadId);

        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        /// <returns>Download</returns>
        Task<Download> GetDownloadByGuid(Guid downloadGuid);

        /// <summary>
        /// Deletes a download
        /// </summary>
        /// <param name="download">Download</param>
        Task DeleteDownload(Download download);

        /// <summary>
        /// Inserts a download
        /// </summary>
        /// <param name="download">Download</param>
        Task InsertDownload(Download download);

        /// <summary>
        /// Updates the download
        /// </summary>
        /// <param name="download">Download</param>
        Task UpdateDownload(Download download);

        /// <summary>
        /// Gets a value indicating whether download is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItem">Order item to check</param>
        /// <param name="product">Product</param>
        /// <returns>True if download is allowed; otherwise, false.</returns>
        bool IsDownloadAllowed(Order order, OrderItem orderItem, Product product);

        /// <summary>
        /// Gets a value indicating whether license download is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItem">Order item to check</param>
        /// <param name="product">Product</param>
        /// <returns>True if license download is allowed; otherwise, false.</returns>
        bool IsLicenseDownloadAllowed(Order order, OrderItem orderItem, Product product);

    }
}
