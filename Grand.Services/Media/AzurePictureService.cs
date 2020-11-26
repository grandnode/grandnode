using Azure.Storage.Blobs;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Domain.Data;
using Grand.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Logging;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Grand.Services.Media
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class AzurePictureService : PictureService
    {
        #region Fields

        private static BlobContainerClient container = null;

        private readonly GrandConfig _config;
        #endregion

        #region Ctor

        public AzurePictureService(IRepository<Picture> pictureRepository,
            ISettingService settingService,
            ILogger logger,
            IMediator mediator,
            IWebHostEnvironment hostingEnvironment,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            MediaSettings mediaSettings,
            GrandConfig config)
            : base(pictureRepository,
                settingService,
                logger,
                mediator,
                hostingEnvironment,
                storeContext,
                cacheManager,
                mediaSettings)
        {
            _config = config;

            if (string.IsNullOrEmpty(_config.AzureBlobStorageConnectionString))
                throw new Exception("Azure connection string for BLOB is not specified");
            if (string.IsNullOrEmpty(_config.AzureBlobStorageContainerName))
                throw new Exception("Azure container name for BLOB is not specified");
            if (string.IsNullOrEmpty(_config.AzureBlobStorageEndPoint))
                throw new Exception("Azure end point for BLOB is not specified");

            container = new BlobContainerClient(_config.AzureBlobStorageConnectionString, _config.AzureBlobStorageContainerName);

        }

        #endregion

        #region Utilities

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async Task DeletePictureThumbs(Picture picture)
        {
            string filter = string.Format("{0}", picture.Id);
            var blobs = container.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, filter);

            foreach (var blob in blobs)
            {
                await container.DeleteBlobAsync(blob.Name);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            var thumbFilePath = _config.AzureBlobStorageEndPoint + _config.AzureBlobStorageContainerName + "/" + thumbFileName;
            return thumbFilePath;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            var url = _config.AzureBlobStorageEndPoint + _config.AzureBlobStorageContainerName + "/";
            url += thumbFileName;
            return url;
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override async Task<bool> GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            try
            {
                await foreach (var blob in container.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.All, thumbFileName))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="binary">Picture binary</param>
        protected override Task SaveThumb(string thumbFilePath, string thumbFileName, byte[] binary)
        {
            Stream stream = new MemoryStream(binary);
            container.UploadBlob(thumbFileName, stream);
            return Task.CompletedTask;
        }

        #endregion
    }
}
