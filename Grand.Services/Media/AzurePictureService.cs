using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Domain.Data;
using Grand.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Logging;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Grand.Services.Media
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class AzurePictureService : PictureService
    {
        #region Fields

        private static CloudStorageAccount _storageAccount = null;
        private static CloudBlobClient blobClient = null;
        private static CloudBlobContainer container_thumb = null;

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

            _storageAccount = CloudStorageAccount.Parse(_config.AzureBlobStorageConnectionString);
            if (_storageAccount == null)
                throw new Exception("Azure connection string for BLOB is not wrong");

            //should we do it for each HTTP request?
            blobClient = _storageAccount.CreateCloudBlobClient();
        }

        #endregion

        #region Utilities

        protected async Task InitContainerThumb()
        {
            if (container_thumb == null)
            {
                var containerPermissions = new BlobContainerPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                container_thumb = blobClient.GetContainerReference(_config.AzureBlobStorageContainerName);
                await container_thumb.CreateIfNotExistsAsync();
                await container_thumb.SetPermissionsAsync(containerPermissions);
            }
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async Task DeletePictureThumbs(Picture picture)
        {
            await InitContainerThumb();

            BlobContinuationToken continuationToken = null;
            string filter = string.Format("{0}", picture.Id);
            var files = await container_thumb.ListBlobsSegmentedAsync(filter, true, BlobListingDetails.All, int.MaxValue, continuationToken, null, null);

            foreach (var ff in files.Results)
            {
                CloudBlockBlob blockBlob = (CloudBlockBlob)ff;
                await blockBlob.DeleteAsync();
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

            url = url + thumbFileName;
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
                await InitContainerThumb();
                CloudBlockBlob blockBlob = container_thumb.GetBlockBlobReference(thumbFileName);
                return await blockBlob.ExistsAsync();
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
            InitContainerThumb().Wait();
            CloudBlockBlob blockBlob = container_thumb.GetBlockBlobReference(thumbFileName);
            blockBlob.UploadFromByteArrayAsync(binary, 0, binary.Length).Wait();
            return Task.CompletedTask;
        }

        #endregion
    }
}
