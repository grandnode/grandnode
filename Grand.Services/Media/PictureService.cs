using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Logging;
using Grand.Services.Seo;
using SkiaSharp;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial class PictureService : IPictureService
    {
        #region Const

        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 3;

        #endregion

        #region Fields

        private readonly IRepository<Picture> _pictureRepository;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly MediaSettings _mediaSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pictureRepository">Picture repository</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="logger">Logger</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="mediaSettings">Media settings</param>
        public PictureService(IRepository<Picture> pictureRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            ILogger logger,
            IMediator mediator,
            MediaSettings mediaSettings,
            IWebHostEnvironment hostingEnvironment)
        {
            _pictureRepository = pictureRepository;
            _settingService = settingService;
            _webHelper = webHelper;
            _logger = logger;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Returns the file extension from mime type.
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <returns>File extension</returns>
        protected virtual string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            string[] parts = mimeType.Split('/');
            string lastPart = parts[parts.Length - 1];
            switch (lastPart)
            {
                case "pjpeg":
                    lastPart = "jpg";
                    break;
                case "x-png":
                    lastPart = "png";
                    break;
                case "x-icon":
                    lastPart = "ico";
                    break;
            }
            return lastPart;
        }

        /// <summary>
        /// Loads a picture from file
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary</returns>
        protected virtual byte[] LoadPictureFromFile(string pictureId, string mimeType)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string fileName = string.Format("{0}_0.{1}", pictureId, lastPart);
            var filePath = GetPictureLocalPath(fileName);
            if (!File.Exists(filePath))
                return new byte[0];
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Save picture on file system
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        protected virtual void SavePictureInFile(string pictureId, byte[] pictureBinary, string mimeType)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string fileName = string.Format("{0}_0.{1}", pictureId, lastPart);
            File.WriteAllBytes(GetPictureLocalPath(fileName), pictureBinary);
        }

        /// <summary>
        /// Delete a picture on file system
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual void DeletePictureOnFileSystem(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string fileName = string.Format("{0}_0.{1}", picture.Id, lastPart);
            string filePath = GetPictureLocalPath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual void DeletePictureThumbs(Picture picture)
        {
            string filter = string.Format("{0}*.*", picture.Id);
            var thumbDirectoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "content/images/thumbs");
            string[] currentFiles = System.IO.Directory.GetFiles(thumbDirectoryPath, filter, SearchOption.AllDirectories);
            foreach (string currentFileName in currentFiles)
            {
                var thumbFilePath = GetThumbLocalPath(currentFileName);
                File.Delete(thumbFilePath);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbLocalPath(string thumbFileName)
        {
            var thumbsDirectoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "content/images/thumbs");
            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                    thumbsDirectoryPath = Path.Combine(thumbsDirectoryPath, subDirectoryName);
                    if (!System.IO.Directory.Exists(thumbsDirectoryPath))
                    {
                        System.IO.Directory.CreateDirectory(thumbsDirectoryPath);
                    }
                }
            }
            var thumbFilePath = Path.Combine(thumbsDirectoryPath, thumbFileName);
            return thumbFilePath;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            storeLocation = !String.IsNullOrEmpty(storeLocation)
                                    ? storeLocation
                                    : _webHelper.GetStoreLocation();
            var url = storeLocation + "content/images/thumbs/";

            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                    url = url + subDirectoryName + "/";
                }
            }

            url = url + thumbFileName;
            return url;
        }

        /// <summary>
        /// Get picture local path. Used when images stored on file system (not in the database)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetPictureLocalPath(string fileName)
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, "content/images", fileName);
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        protected virtual async Task<byte[]> LoadPictureBinaryAsync(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var result = fromDb
                ? (await _pictureRepository.GetByIdAsync(picture.Id)).PictureBinary
                : LoadPictureFromFile(picture.Id, picture.MimeType);

            return result;
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        protected virtual byte[] LoadPictureBinary(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var result = fromDb
                ? (_pictureRepository.GetById(picture.Id)).PictureBinary
                : LoadPictureFromFile(picture.Id, picture.MimeType);

            return result;
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected virtual bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            return File.Exists(thumbFilePath);
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="binary">Picture binary</param>
        protected virtual void SaveThumb(string thumbFilePath, string thumbFileName, byte[] binary)
        {
            File.WriteAllBytes(thumbFilePath, binary);

        }


        #endregion

        #region Getting picture local path/URL methods

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        public virtual async Task<byte[]> LoadPictureBinary(Picture picture)
        {
            return await LoadPictureBinaryAsync(picture, this.StoreInDb);
        }

        /// <summary>
        /// Get picture SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Result</returns>
        public virtual string GetPictureSeName(string name)
        {
            return SeoExtensions.GetSeName(name, true, false);
        }

        /// <summary>
        /// Gets the default picture URL
        /// </summary>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Picture URL</returns>
        public virtual async Task<string> GetDefaultPictureUrl(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null)
        {
            string defaultImageFileName;
            switch (defaultPictureType)
            {
                case PictureType.Avatar:
                    defaultImageFileName = _settingService.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
                    break;
                case PictureType.Entity:
                default:
                    defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
                    break;
            }

            string filePath = GetPictureLocalPath(defaultImageFileName);

            if (!File.Exists(filePath))
            {
                return "";
            }
            if (targetSize == 0)
            {
                string url = (!string.IsNullOrEmpty(storeLocation)
                                 ? storeLocation
                                 : _webHelper.GetStoreLocation())
                                 + "content/images/" + defaultImageFileName;
                return url;
            }
            else
            {
                string fileExtension = Path.GetExtension(filePath);
                string thumbFileName = string.Format("{0}_{1}{2}",
                    Path.GetFileNameWithoutExtension(filePath),
                    targetSize,
                    fileExtension);
                var thumbFilePath = GetThumbLocalPath(thumbFileName);

                using (var mutex = new Mutex(false, thumbFileName))
                {
                    if (GeneratedThumbExists(thumbFilePath, thumbFileName))
                        return GetThumbUrl(thumbFileName, storeLocation);

                    mutex.WaitOne();
                    using (var image = SKBitmap.Decode(filePath))
                    {
                        var pictureBinary = await ApplyResize(image, targetSize);
                        SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                    }
                    mutex.ReleaseMutex();
                }
                var url = GetThumbUrl(thumbFileName, storeLocation);
                return url;
            }
        }

        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        public virtual async Task<string> GetPictureUrl(string pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = await GetPictureById(pictureId);
            return await GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        public virtual async Task<string> GetPictureUrl(Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            string url = string.Empty;
            byte[] pictureBinary = null;
            if (picture != null)
                pictureBinary = await LoadPictureBinary(picture);

            if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
            {
                if (showDefaultPicture)
                {
                    url = await GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                }
                return url;
            }

            string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string thumbFileName;
            if (picture.IsNew)
            {
                DeletePictureThumbs(picture);

                //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                picture = await UpdatePicture(picture.Id,
                    pictureBinary,
                    picture.MimeType,
                    picture.SeoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    false,
                    false);
            }

            string seoFileName = picture.SeoFilename;
            if (targetSize == 0)
            {
                thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                    string.Format("{0}_{1}.{2}", picture.Id, seoFileName, lastPart) :
                    string.Format("{0}.{1}", picture.Id, lastPart);
                var thumbFilePath = GetThumbLocalPath(thumbFileName);
                using (var mutex = new System.Threading.Mutex(false, thumbFileName))
                {
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        mutex.WaitOne();
                        SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                        mutex.ReleaseMutex();
                    }
                }
            }
            else
            {
                thumbFileName = !string.IsNullOrEmpty(seoFileName) ?
                    string.Format("{0}_{1}_{2}.{3}", picture.Id, seoFileName, targetSize, lastPart) :
                    string.Format("{0}_{1}.{2}", picture.Id, targetSize, lastPart);
                var thumbFilePath = GetThumbLocalPath(thumbFileName);
                using (var mutex = new Mutex(false, thumbFileName))
                {
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        mutex.WaitOne();
                        using (var image = SKBitmap.Decode(pictureBinary))
                        {
                            pictureBinary = await ApplyResize(image, targetSize);
                        }
                        SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                        mutex.ReleaseMutex();
                    }
                }
            }
            url = GetThumbUrl(thumbFileName, storeLocation);
            return url;
        }

        /// <summary>
        /// Get a picture local path
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <returns></returns>
        public virtual async Task<string> GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
        {
            string url = await GetPictureUrl(picture, targetSize, showDefaultPicture);
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return GetThumbLocalPath(Path.GetFileName(url));
        }

        #endregion

        #region CRUD methods

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual Task<Picture> GetPictureById(string pictureId)
        {
            return _pictureRepository.GetByIdAsync(pictureId);
        }

        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        public virtual async Task DeletePicture(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            //delete thumbs
            DeletePictureThumbs(picture);

            //delete from file system
            if (!this.StoreInDb)
                DeletePictureOnFileSystem(picture);

            //delete from database
            await _pictureRepository.DeleteAsync(picture);

            //event notification
            await _mediator.EntityDeleted(picture);
        }

        /// <summary>
        /// Clears only physical Picture files located at ~/wwwroot/content/images/thumbs/, it won't affect Pictures stored in database
        /// </summary>
        public virtual async Task ClearThumbs()
        {
            const string searchPattern = "*.*";
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "content/images/thumbs");

            if (!System.IO.Directory.Exists(path))
                return;

            foreach (string str in System.IO.Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories))
            {
                if (str.Contains("placeholder.txt"))
                    continue;
                try
                {
                    File.Delete(this.GetThumbLocalPath(str));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets a collection of pictures
        /// </summary>
        /// <param name="pageIndex">Current page</param>
        /// <param name="pageSize">Items on each page</param>
        /// <returns>Paged list of pictures</returns>
        public virtual IPagedList<Picture> GetPictures(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _pictureRepository.Table
                        orderby p.Id descending
                        select p;
            var pictures = new PagedList<Picture>(query, pageIndex, pageSize);
            return pictures;
        }

        /// <summary>
        /// Inserts a picture
        /// </summary>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="altAttribute">"alt" attribute for "img" HTML element</param>
        /// <param name="titleAttribute">"title" attribute for "img" HTML element</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        public virtual async Task<Picture> InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = await ValidatePicture(pictureBinary, mimeType);

            var picture = new Picture {
                PictureBinary = this.StoreInDb ? pictureBinary : new byte[0],
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew,
            };
            await _pictureRepository.InsertAsync(picture);

            if (!this.StoreInDb)
                SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            await _mediator.EntityInserted(picture);

            return picture;
        }

        /// <summary>
        /// Updates the picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="altAttribute">"alt" attribute for "img" HTML element</param>
        /// <param name="titleAttribute">"title" attribute for "img" HTML element</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        public virtual async Task<Picture> UpdatePicture(string pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = await ValidatePicture(pictureBinary, mimeType);

            var picture = await GetPictureById(pictureId);
            if (picture == null)
                return null;

            //delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            picture.PictureBinary = this.StoreInDb ? pictureBinary : new byte[0];
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;

            await _pictureRepository.UpdateAsync(picture);

            if (!this.StoreInDb)
                SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            await _mediator.EntityUpdated(picture);

            return picture;
        }

        /// <summary>
        /// Updates a SEO filename of a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <returns>Picture</returns>
        public virtual async Task<Picture> SetSeoFilename(string pictureId, string seoFilename)
        {
            var picture = await GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            //update if it has been changed
            if (seoFilename != picture.SeoFilename)
            {
                //update picture
                picture = await UpdatePicture(picture.Id,
                    await LoadPictureBinary(picture),
                    picture.MimeType,
                    seoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    true,
                    false);
            }
            return picture;
        }

        /// <summary>
        /// Validates input picture dimensions
        /// </summary>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary or throws an exception</returns>
        public virtual async Task<byte[]> ValidatePicture(byte[] byteArray, string mimeType)
        {
            try
            {
                using (var ms = new MemoryStream(byteArray))
                {
                    using (var image = SKBitmap.Decode(byteArray))
                    {
                        if (image.Width >= image.Height)
                        {
                            //horizontal rectangle or square
                            if (image.Width > _mediaSettings.MaximumImageSize && image.Height > _mediaSettings.MaximumImageSize)
                                byteArray = await ApplyResize(image, _mediaSettings.MaximumImageSize);
                        }
                        else if (image.Width < image.Height)
                        {
                            //vertical rectangle
                            if (image.Width > _mediaSettings.MaximumImageSize)
                                byteArray = await ApplyResize(image, _mediaSettings.MaximumImageSize);
                        }
                        return byteArray;
                    }
                }
            }
            catch
            {
                return byteArray;
            }
        }

        protected async Task<byte[]> ApplyResize(SKBitmap image, int targetSize)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (targetSize <= 0)
            {
                targetSize = 800;
            }
            float width, height;
            if (image.Height > image.Width)
            {
                // portrait
                width = image.Width * (targetSize / (float)image.Height);
                height = targetSize;
            }
            else
            {
                // landscape or square
                width = targetSize;
                height = image.Height * (targetSize / (float)image.Width);
            }

            if ((int)width == 0 || (int)height == 0)
            {
                width = image.Width;
                height = image.Height;
            }

            using (var resized = image.Resize(new SKImageInfo((int)width, (int)height), SKFilterQuality.High))
            {
                using (var resimage = SKImage.FromBitmap(resized))
                {
                    return await Task.FromResult(resimage.Encode().ToArray());
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the images should be stored in data base.
        /// </summary>
        public virtual bool StoreInDb {
            get {
                return _settingService.GetSettingByKey("Media.Images.StoreInDB", true);
            }
            set {
                //check whether it's a new value
                if (this.StoreInDb == value)
                    return;

                //save the new setting value
                _settingService.SetSetting("Media.Images.StoreInDB", value).GetAwaiter().GetResult();

                int pageIndex = 0;
                const int pageSize = 400;
                try
                {

                    while (true)
                    {
                        var pictures = this.GetPictures(pageIndex, pageSize);
                        pageIndex++;
                        if (!pictures.Any())
                            break;

                        foreach (var picture in pictures)
                        {
                            var pictureBinary = LoadPictureBinary(picture, !value);

                            if (value)
                                DeletePictureOnFileSystem(picture);
                            else
                                //now on file system
                                SavePictureInFile(picture.Id, pictureBinary, picture.MimeType);
                            picture.PictureBinary = value ? pictureBinary : new byte[0];
                            picture.IsNew = true;
                        }
                        //save all at once
                        _pictureRepository.Update(pictures);
                    }
                }
                finally
                {
                }
            }
        }

        #endregion
    }
}