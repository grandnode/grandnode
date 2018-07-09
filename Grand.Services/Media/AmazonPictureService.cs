using System;
using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Logging;
using System.IO;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using Microsoft.AspNetCore.Hosting;

namespace Grand.Services.Media
{
    /// <summary>
    /// Picture service for Amazon
    /// </summary>
    public partial class AmazonPictureService : PictureService
    {
        #region Fields

        private readonly GrandConfig _config;
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        #endregion

        #region Ctor

        public AmazonPictureService(IRepository<Picture> pictureRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            ILogger logger,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IHostingEnvironment hostingEnvironment,
            GrandConfig config)
            : base(pictureRepository,
                settingService,
                webHelper,
                logger,
                eventPublisher,
                mediaSettings, 
                hostingEnvironment)
        {
            this._config = config;

            //Arguments guard
            if (string.IsNullOrEmpty(_config.AmazonAwsAccessKeyId))
                throw new ArgumentNullException("AmazonAwsAccessKeyId");
            if (string.IsNullOrEmpty(_config.AmazonAwsSecretAccessKey))
                throw new ArgumentNullException("AmazonAwsSecretAccessKey");
            if (string.IsNullOrEmpty(_config.AmazonBucketName))
                throw new ArgumentNullException("AmazonBucketName");

            //Region guard
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(_config.AmazonRegion);
            if (regionEndpoint.DisplayName == "Unknown")
                throw new NullReferenceException("specified Region is invalid");

            //Client guard
            _s3Client = new AmazonS3Client(_config.AmazonAwsAccessKeyId, _config.AmazonAwsSecretAccessKey, regionEndpoint);
            try
            {
                EnsureValidResponse(_s3Client.ListBucketsAsync().Result, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            //Bucket guard
            _bucketName = _config.AmazonBucketName;
            var bucketExists = _s3Client.DoesS3BucketExistAsync(_bucketName).Result;
            while (bucketExists == false)
            {
                S3Region s3region = S3Region.FindValue(_config.AmazonRegion);
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = _bucketName,
                    BucketRegion = s3region,
                };

                try
                {
                    EnsureValidResponse(_s3Client.PutBucketAsync(putBucketRequest).Result, HttpStatusCode.OK);

                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode == "BucketAlreadyOwnedByYou")
                        break;

                    throw;
                }
                bucketExists = _s3Client.DoesS3BucketExistAsync(_bucketName).Result;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Ensure Every Response Will Have Expected HttpStatusCode
        /// </summary>
        /// <param name="response">Actual Response</param>
        /// <param name="validHttpStatusCode">Expected Status Code</param>
        private void EnsureValidResponse(AmazonWebServiceResponse actualResponse, HttpStatusCode expectedHttpStatusCode)
        {
            if (actualResponse.HttpStatusCode != expectedHttpStatusCode)
                throw new Exception("Http Status Codes Aren't Consistent");
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override void DeletePictureThumbs(Picture picture)
        {
            var listObjectsRequest = new ListObjectsV2Request()
            {
                BucketName = _bucketName,
                Prefix = picture.Id
            };
            var listObjectsResponse = _s3Client.ListObjectsV2Async(listObjectsRequest).Result;

            foreach (var s3Object in listObjectsResponse.S3Objects)
            {
                EnsureValidResponse(_s3Client.DeleteObjectAsync(_bucketName, s3Object.Key).Result, HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            var url = string.Format("http://{0}.s3.amazonAws.com/{1}", _bucketName, thumbFileName);
            return url;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            var url = string.Format("http://{0}.s3.amazonAws.com/{1}", _bucketName, thumbFileName);
            return url;
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            try
            {
                var getObjectResponse = _s3Client.GetObjectAsync(_bucketName, thumbFileName).Result;
                EnsureValidResponse(getObjectResponse, HttpStatusCode.OK);

                if (getObjectResponse.BucketName != _bucketName && getObjectResponse.Key != thumbFileName)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path (unused in Amazon S3)</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="binary">Picture binary</param>
        protected override void SaveThumb(string thumbFilePath, string thumbFileName, byte[] binary)
        {
            using (Stream stream = new MemoryStream(binary))
            {
                var putObjectRequest = new PutObjectRequest()
                {
                    BucketName = _bucketName,
                    InputStream = stream,
                    Key = thumbFileName,
                    StorageClass = S3StorageClass.Standard,
                };
                EnsureValidResponse(_s3Client.PutObjectAsync(putObjectRequest).Result, HttpStatusCode.OK);
            }
            _s3Client.MakeObjectPublicAsync(_bucketName, thumbFileName, true);
        }

        /// <summary>
        /// Clears pictures stored on Amazon S3, it won't affect Pictures stored in database
        /// </summary>
        public override void ClearThumbs()
        {
            var listObjectsRequest = new ListObjectsV2Request()
            {
                BucketName = _bucketName
            };
            var listObjectsResponse = _s3Client.ListObjectsV2Async(listObjectsRequest).Result;

            foreach (var s3Object in listObjectsResponse.S3Objects)
            {
                EnsureValidResponse(_s3Client.DeleteObjectAsync(_bucketName, s3Object.Key).Result, HttpStatusCode.NoContent);
            }
        }

        #endregion
    }
}