using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Logging;
using Grand.Services.Seo;
using ImageSharp;
using ImageSharp.Drawing;
using ImageSharp.PixelFormats;
using ImageSharp.Processing;
using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static readonly object s_lock = new object();

        private readonly IRepository<Picture> _pictureRepository;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pictureRepository">Picture repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="logger">Logger</param>
        /// <param name="dbContext">Database context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="mediaSettings">Media settings</param>
        public PictureService(IRepository<Picture> pictureRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            ILogger logger,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IHostingEnvironment hostingEnvironment)
        {
            this._pictureRepository = pictureRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._logger = logger;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
            this._hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates picture dimensions whilst maintaining aspect
        /// </summary>
        /// <param name="originalSize">The original picture size</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="resizeType">Resize type</param>
        /// <param name="ensureSizePositive">A value indicatingh whether we should ensure that size values are positive</param>
        /// <returns></returns>wy ludzie gracie w dziwn¹ grê polegaj¹c¹ na wzajemnym poni¿aniu siê i udowadnianiu wy¿szoœci. Mo¿e to by³o u¿yteczne gdy jeszcze byliœcie ma³pami
        protected virtual Size CalculateDimensions(Size originalSize, int targetSize,
            ResizeType resizeType = ResizeType.LongestSide, bool ensureSizePositive = true)
        {
            float width, height;

            switch (resizeType)
            {
                case ResizeType.LongestSide:
                    if (originalSize.Height > originalSize.Width)
                    {
                        // portrait
                        width = originalSize.Width * (targetSize / (float)originalSize.Height);
                        height = targetSize;
                    }
                    else
                    {
                        // landscape or square
                        width = targetSize;
                        height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    }
                    break;
                case ResizeType.Width:
                    width = targetSize;
                    height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    break;
                case ResizeType.Height:
                    width = originalSize.Width * (targetSize / (float)originalSize.Height);
                    height = targetSize;
                    break;
                default:
                    throw new Exception("Not supported ResizeType");
            }

            if (ensureSizePositive)
            {
                if (width < 1)
                    width = 1;
                if (height < 1)
                    height = 1;
            }

            //we invoke Math.Round to ensure that no white background is rendered 
            return new Size((int)Math.Round(width), (int)Math.Round(height));
        }

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
            var thumbDirectoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "content\\images\\thumbs");
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
            var thumbsDirectoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "content\\images\\thumbs");
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
            var url = storeLocation + "content\\images\\thumbs\\";

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
        /// <param name="imagesDirectoryPath">Directory path with images; if null, then default one is used</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetPictureLocalPath(string fileName)
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, "content\\images", fileName);
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
                ? _pictureRepository.GetById(picture.Id).PictureBinary
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

            if (_mediaSettings.UseImageCompress)
            {
                var path = Path.Combine(_hostingEnvironment.WebRootPath, "content\\images\\thumbs");
                var processStartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "pingo.exe",
                    Arguments = string.Format("-s4 {0}", thumbFileName),
                    WorkingDirectory = path,
                    CreateNoWindow = true,
                };

                try
                {
                    System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    _logger.Error(ex.Message);
                }
            }
        }


        #endregion

        #region Getting picture local path/URL methods

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        public virtual byte[] LoadPictureBinary(Picture picture)
        {
            return LoadPictureBinary(picture, this.StoreInDb);
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
        public virtual string GetDefaultPictureUrl(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null,
            bool applyWatermarkForSpecified = false)
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
                string url = (!String.IsNullOrEmpty(storeLocation)
                                 ? storeLocation
                                 : _webHelper.GetStoreLocation())
                                 + "content\\images\\" + defaultImageFileName;
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

                using (var mutex = new System.Threading.Mutex(false, thumbFileName))
                {
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        mutex.WaitOne();

                        if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                        {
                            using (var b = Image.Load(filePath))
                            {
                                var pictureBinary = File.ReadAllBytes(filePath);

                                pictureBinary = ApplyResize(pictureBinary, targetSize, new Size(b.Width, b.Height));
                                b.Dispose();

                                //these 2 pictureBinar'ies aren't the same byte array, assignation is needed
                                if (targetSize >= _mediaSettings.WatermarkForPicturesAboveSize && applyWatermarkForSpecified)
                                {
                                    pictureBinary = ApplyWatermark(pictureBinary);
                                }

                                SaveThumb(thumbFilePath, thumbFileName, pictureBinary);

                            }
                            mutex.ReleaseMutex();
                        }
                    }
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
        public virtual string GetPictureUrl(string pictureId,
            bool applyWatermarkForSpecified = false,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = GetPictureById(pictureId);
            return GetPictureUrl(picture, applyWatermarkForSpecified, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
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
        public virtual string GetPictureUrl(Picture picture,
            bool applyWatermarkForSpecified = false,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            lock (s_lock)
            {
                string url = string.Empty;
                byte[] pictureBinary = null;
                if (picture != null)
                    pictureBinary = LoadPictureBinary(picture);
                if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
                {
                    if (showDefaultPicture)
                    {
                        url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation, applyWatermarkForSpecified);
                    }
                    return url;
                }

                string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
                string thumbFileName;
                if (picture.IsNew)
                {
                    DeletePictureThumbs(picture);

                    //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                    picture = UpdatePicture(picture.Id,
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

                            if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                            {
                                {
                                    if (_mediaSettings.ApplyWatermarkOnPicturesWithOriginalSize && applyWatermarkForSpecified)
                                    {
                                        using (var stream = new MemoryStream(pictureBinary))
                                        {
                                            Image<Rgba32> b = null;
                                            try
                                            {
                                                //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                                b = Image.Load(stream);
                                            }
                                            catch (ArgumentException exc)
                                            {
                                                _logger.Error(string.Format("Error generating picture thumb. ID={0}", picture.Id), exc);
                                            }
                                            if (b == null)
                                            {
                                                //bitmap could not be loaded for some reasons
                                                return url;
                                            }

                                            //input pictureBinary isn't the same output pictureBinary (different MemoryStreams)
                                            pictureBinary = ApplyWatermark(pictureBinary);
                                        }
                                    }
                                    SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                                }
                            }
                            mutex.ReleaseMutex();
                        }
                    }
                }
                else
                {
                    thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                        string.Format("{0}_{1}_{2}.{3}", picture.Id, seoFileName, targetSize, lastPart) :
                        string.Format("{0}_{1}.{2}", picture.Id, targetSize, lastPart);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName);
                    using (var mutex = new System.Threading.Mutex(false, thumbFileName))
                    {
                        if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                        {
                            mutex.WaitOne();

                            if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                            {
                                {
                                    using (var stream = new MemoryStream(pictureBinary))
                                    {
                                        Image<Rgba32> b = null;
                                        try
                                        {
                                            b = Image.Load(stream);
                                        }
                                        catch (ArgumentException exc)
                                        {
                                            _logger.Error(string.Format("Error generating picture thumb. ID={0}", picture.Id), exc);
                                        }
                                        if (b == null)
                                        {
                                            return url;
                                        }

                                        pictureBinary = ApplyResize(pictureBinary, targetSize, new Size(b.Width, b.Height));
                                        b.Dispose();

                                        if (targetSize >= _mediaSettings.WatermarkForPicturesAboveSize && applyWatermarkForSpecified)
                                        {
                                            pictureBinary = ApplyWatermark(pictureBinary);
                                        }

                                        SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                                    }
                                }
                            }
                        }
                    }
                }
                url = GetThumbUrl(thumbFileName, storeLocation);
                return url;
            }
        }

        /// <summary>
        /// Get a picture local path
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <returns></returns>
        public virtual string GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
        {
            string url = GetPictureUrl(picture, false, targetSize, showDefaultPicture);
            if (String.IsNullOrEmpty(url))
                return String.Empty;

            return GetThumbLocalPath(Path.GetFileName(url));
        }

        #endregion

        #region CRUD methods

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual Picture GetPictureById(string pictureId)
        {
            return _pictureRepository.GetById(pictureId);
        }

        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        public virtual void DeletePicture(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            //delete thumbs
            DeletePictureThumbs(picture);

            //delete from file system
            if (!this.StoreInDb)
                DeletePictureOnFileSystem(picture);

            //delete from database
            _pictureRepository.Delete(picture);

            //event notification
            _eventPublisher.EntityDeleted(picture);
        }

        /// <summary>
        /// Clears only physical Picture files located at ~/wwwroot/content/images/thumbs/, it won't affect Pictures stored in database
        /// </summary>
        public virtual void ClearThumbs()
        {
            const string searchPattern = "*.*";
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "content\\images\\thumbs");

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
            var pics = new PagedList<Picture>(query, pageIndex, pageSize);
            return pics;
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
        public virtual Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = new Picture
            {
                PictureBinary = this.StoreInDb ? pictureBinary : new byte[0],
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew,
            };
            _pictureRepository.Insert(picture);

            if (!this.StoreInDb)
                SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            _eventPublisher.EntityInserted(picture);

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
        public virtual Picture UpdatePicture(string pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = ValidatePicture(pictureBinary, mimeType);

            var picture = GetPictureById(pictureId);
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

            _pictureRepository.Update(picture);

            if (!this.StoreInDb)
                SavePictureInFile(picture.Id, pictureBinary, mimeType);

            //event notification
            _eventPublisher.EntityUpdated(picture);

            return picture;
        }

        /// <summary>
        /// Updates a SEO filename of a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <returns>Picture</returns>
        public virtual Picture SetSeoFilename(string pictureId, string seoFilename)
        {
            var picture = GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            //update if it has been changed
            if (seoFilename != picture.SeoFilename)
            {
                //update picture
                picture = UpdatePicture(picture.Id,
                    LoadPictureBinary(picture),
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
        public virtual byte[] ValidatePicture(byte[] byteArray, string mimeType)
        {
            var image = Image.Load(byteArray);

            using (MemoryStream ms = new MemoryStream())
            {
                if (image.Width >= image.Height)
                {
                    //horizontal rectangle or square
                    if (image.Width > _mediaSettings.MaximumImageSize && image.Height > _mediaSettings.MaximumImageSize)
                        byteArray = ApplyResize(byteArray, _mediaSettings.MaximumImageSize);
                }
                else if (image.Width < image.Height)
                {
                    //vertical rectangle
                    if (image.Width > _mediaSettings.MaximumImageSize)
                        byteArray = ApplyResize(byteArray, _mediaSettings.MaximumImageSize);
                }
                return byteArray;
            };
        }

        protected byte[] ApplyResize(byte[] byteArray, int targetSize, Size originalSize = default(Size))
        {
            var image = Image.Load(byteArray);

            var size = default(Size);
            if (originalSize != default(Size))
            {
                size = CalculateDimensions(originalSize, targetSize);
            }
            else
            {
                size = new Size(targetSize, targetSize);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                image
                    .Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = size,
                    })
                    .Save(ms, ImageFormats.Jpeg);
                return ms.ToArray();
            };
        }

        protected byte[] ApplyWatermark(byte[] pictureBinary)
        {
            //performance optimization: no Watermark Text and Watermark Overlay ? Return
            if (string.IsNullOrEmpty(_mediaSettings.WatermarkText)
                && string.IsNullOrEmpty(_mediaSettings.WatermarkOverlayID)
                && _mediaSettings.WatermarkOverlayID == "0")
                return pictureBinary;

            //this piece of code needs to be here, otherwise "Input stream is not a supported format" Exception
            var pictureWidth = default(int);
            var pictureHeight = default(int);
            using (MemoryStream pictureStream = new MemoryStream(pictureBinary))
            {
                var bitmap = Image.Load(pictureStream);
                pictureWidth = bitmap.Width;
                pictureHeight = bitmap.Height;
                pictureStream.Dispose();
            }

            using (MemoryStream inStream = new MemoryStream(pictureBinary))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    var calculatedHorizontalPixel = (pictureWidth * _mediaSettings.WatermarkPositionXPercent) / 100;
                    var calculatedVerticalPixel = (pictureHeight * _mediaSettings.WatermarkPositionYPercent) / 100;
                    var calculatedFontSize = (pictureHeight * _mediaSettings.WatermarkFontSizePercent) / 100;

                    var collection = new FontCollection();
                    var fontDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "content\\watermark-fonts");
                    var ttfFiles = new DirectoryInfo(fontDirectory).EnumerateFiles();
                    foreach (var ttf in ttfFiles)
                    {
                        collection.Install(ttf.FullName);
                    }

                    var fontFamilies = new List<FontFamily>(collection.Families);

                    //TODO: give user a possibility to change watermark text font
                    var font = fontFamilies[0].CreateFont(calculatedFontSize);

                    using (Image<Rgba32> image = new Image<Rgba32>(Image.Load(inStream)))
                    {
                        //text watermark
                        if (!string.IsNullOrEmpty(_mediaSettings.WatermarkText))
                        {
                            image
                                .DrawText(
                                _mediaSettings.WatermarkText,
                                font,
                                _mediaSettings.WatermarkFontColor,
                                new PointF(calculatedHorizontalPixel, calculatedVerticalPixel),
                                new TextGraphicsOptions
                                {
                                    BlenderMode = PixelBlenderMode.Normal,
                                }
                            );
                        }
                        //image watermark
                        if (!(string.IsNullOrEmpty(_mediaSettings.WatermarkOverlayID) || _mediaSettings.WatermarkOverlayID == "0"))
                        {
                            var overlayWidth = default(int);
                            var overlayHeight = default(int);
                            //picture is a horizontal rectangle, so Overlay will be tailored in context of shorter dimension - Vertical Height
                            if (pictureWidth > pictureHeight)
                            {
                                overlayWidth = (pictureHeight * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                                overlayHeight = (pictureHeight * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                            }
                            //picture is a vertical rectangle, so Overlay will be tailored in context of shorter dimension - Horizontal Width
                            else if (pictureWidth < pictureHeight)
                            {
                                overlayWidth = (pictureWidth * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                                overlayHeight = (pictureWidth * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                            }
                            //picture is a square
                            else if (pictureWidth == pictureHeight)
                            {
                                overlayWidth = (pictureWidth * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                                overlayHeight = (pictureHeight * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                            }

                            //calculate X and Y center of Picture
                            var overlayHalfWidth = overlayWidth / 2;
                            var overlayHalfHeight = overlayHeight / 2;

                            //calculate the absolute X and Y pixel of Picture bitmap
                            //where Overlay should be located
                            var pictureHorizontalPixel = (pictureWidth * _mediaSettings.WatermarkOverlayPositionXPercent) / 100;
                            var pictureVerticalPixel = (pictureHeight * _mediaSettings.WatermarkOverlayPositionYPercent) / 100;

                            //calculate the absolute X and Y pixel of Picture bitmap
                            //where Center of Overlay should be located
                            var overlayHorizontalPosition = pictureHorizontalPixel - overlayHalfWidth;
                            var overlayVerticalPosition = pictureVerticalPixel - overlayHalfHeight;

                            //prevent to get beyond top or left edge of Picture 
                            //(happens when WatermarkOverlaySizePercent is set to low value)
                            if (overlayHorizontalPosition < 0)
                            {
                                overlayHorizontalPosition = 0;
                            }
                            if (overlayVerticalPosition < 0)
                            {
                                overlayVerticalPosition = 0;
                            }

                            var overlayByteArray = LoadPictureBinary(new Picture() { Id = _mediaSettings.WatermarkOverlayID });
                            var overlay = Image.Load(overlayByteArray);
                            var overlayWidthFixed = (image.Width * _mediaSettings.WatermarkOverlaySizePercent) / 100;
                            var overlayHeightFixed = (image.Height * _mediaSettings.WatermarkOverlaySizePercent) / 100;

                            float size = ((float)_mediaSettings.WatermarkOverlayOpacityPercent / (float)100);

                            using (var overlayStream = new MemoryStream(overlayByteArray))
                            {
                                image.DrawImage(
                                        Image.Load(overlayByteArray),
                                        PixelBlenderMode.Normal,
                                        size,
                                        new Size(overlayWidthFixed, overlayHeightFixed),
                                        new Point(overlayHorizontalPosition, overlayVerticalPosition)
                                        );
                                overlayStream.Dispose();
                            }
                        }
                        image
                            .Save(outStream, ImageFormats.Jpeg)
                            .Dispose();
                    }
                    return outStream.ToArray();
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the images should be stored in data base.
        /// </summary>
        public virtual bool StoreInDb
        {
            get
            {
                return _settingService.GetSettingByKey("Media.Images.StoreInDB", true);
            }
            set
            {
                //check whether it's a new value
                if (this.StoreInDb == value)
                    return;

                //save the new setting value
                _settingService.SetSetting("Media.Images.StoreInDB", value);

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