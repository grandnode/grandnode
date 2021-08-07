﻿using Grand.Core;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Media;
using Grand.Services.Configuration;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Grand.Core.Configuration;

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
        private readonly string _distributionDomainName;
        private bool _bucketExist = false;
        private readonly IAmazonS3 _s3Client;

        #endregion

        #region Ctor

        public AmazonPictureService(IRepository<Picture> pictureRepository,
            ISettingService settingService,
            Grand.Services.Logging.ILogger logger,
            IMediator mediator,
            IWebHostEnvironment hostingEnvironment,
            IStoreContext storeContext,
            ICacheBase cacheManager,
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

            //Arguments guard
            if (string.IsNullOrEmpty(_config.AmazonAwsAccessKeyId))
                throw new ArgumentNullException("AmazonAwsAccessKeyId");
            if (string.IsNullOrEmpty(_config.AmazonAwsSecretAccessKey))
                throw new ArgumentNullException("AmazonAwsSecretAccessKey");
            if (string.IsNullOrEmpty(_config.AmazonBucketName))
                throw new ArgumentNullException("AmazonBucketName");

            AmazonS3Config amazonS3Config = new AmazonS3Config();

            if (string.IsNullOrEmpty(_config.AmazonRegionEndpoint))
            {
                //Region guard
                var regionEndpoint = RegionEndpoint.GetBySystemName(_config.AmazonRegion);
                if (regionEndpoint.DisplayName == "Unknown")
                    throw new NullReferenceException("specified Region is invalid");

                amazonS3Config.RegionEndpoint = regionEndpoint;
            }
            else
            {
                amazonS3Config.ServiceURL = _config.AmazonRegionEndpoint;
            }

            //Client guard
            _s3Client = new AmazonS3Client(_config.AmazonAwsAccessKeyId, _config.AmazonAwsSecretAccessKey, amazonS3Config);

            //Bucket guard
            _bucketName = _config.AmazonBucketName;

            //Cloudfront distribution
            _distributionDomainName = _config.AmazonDistributionDomainName;

        }

        #endregion

        #region Utilities

        private async Task CheckBucketExists()
        {
            if (!_bucketExist)
            {
                _bucketExist = await _s3Client.DoesS3BucketExistAsync(_bucketName);
                while (_bucketExist == false)
                {
                    S3Region s3region = S3Region.FindValue(_config.AmazonRegion);
                    var putBucketRequest = new PutBucketRequest {
                        BucketName = _bucketName,
                        BucketRegion = s3region,
                    };

                    try
                    {
                        EnsureValidResponse(await _s3Client.PutBucketAsync(putBucketRequest), HttpStatusCode.OK);

                    }
                    catch (AmazonS3Exception ex)
                    {
                        if (ex.ErrorCode == "BucketAlreadyOwnedByYou")
                            break;

                        throw;
                    }
                    _bucketExist = await _s3Client.DoesS3BucketExistAsync(_bucketName);
                }
            }
        }

        /// <summary>
        /// Ensure Every Response Will Have Expected HttpStatusCode
        /// </summary>
        /// <param name="actualResponse">Actual Response</param>
        /// <param name="expectedHttpStatusCode">Expected Status Code</param>
        private void EnsureValidResponse(AmazonWebServiceResponse actualResponse, HttpStatusCode expectedHttpStatusCode)
        {
            if (actualResponse.HttpStatusCode != expectedHttpStatusCode)
                throw new Exception("Http Status Codes Aren't Consistent");
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async Task DeletePictureThumbs(Picture picture)
        {
            await CheckBucketExists();

            var listObjectsRequest = new ListObjectsV2Request() {
                BucketName = _bucketName,
                Prefix = picture.Id
            };
            var listObjectsResponse = await _s3Client.ListObjectsV2Async(listObjectsRequest);

            foreach (var s3Object in listObjectsResponse.S3Objects)
            {
                EnsureValidResponse(await _s3Client.DeleteObjectAsync(_bucketName, s3Object.Key), HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            if (string.IsNullOrEmpty(_distributionDomainName))
            {
                var url = string.Format("https://{0}.s3.amazonAws.com/{1}", _bucketName, thumbFileName);
                return url;
            }
            else
                return string.Format("https://{0}/{1}", _distributionDomainName, thumbFileName);

        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {

            if (string.IsNullOrEmpty(_distributionDomainName))
            {
                var url = string.Format("https://{0}.s3.amazonAws.com/{1}", _bucketName, thumbFileName);
                return url;
            }
            else
                return string.Format("https://{0}/{1}", _distributionDomainName, thumbFileName);

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
                await CheckBucketExists();
                var getObjectResponse = await _s3Client.GetObjectAsync(_bucketName, thumbFileName);
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
        protected override Task SaveThumb(string thumbFilePath, string thumbFileName, byte[] binary)
        {
            CheckBucketExists().Wait();

            using (Stream stream = new MemoryStream(binary))
            {
                var putObjectRequest = new PutObjectRequest() {
                    BucketName = _bucketName,
                    InputStream = stream,
                    Key = thumbFileName,
                    StorageClass = S3StorageClass.Standard,
                };
                _s3Client.PutObjectAsync(putObjectRequest).Wait();
            }
            _s3Client.MakeObjectPublicAsync(_bucketName, thumbFileName, true).Wait();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears pictures stored on Amazon S3, it won't affect Pictures stored in database
        /// </summary>
        public override async Task ClearThumbs()
        {
            await CheckBucketExists();

            var listObjectsRequest = new ListObjectsV2Request() {
                BucketName = _bucketName
            };
            var listObjectsResponse = await _s3Client.ListObjectsV2Async(listObjectsRequest);

            foreach (var s3Object in listObjectsResponse.S3Objects)
            {
                EnsureValidResponse(await _s3Client.DeleteObjectAsync(_bucketName, s3Object.Key), HttpStatusCode.NoContent);
            }
        }

        #endregion
    }
}