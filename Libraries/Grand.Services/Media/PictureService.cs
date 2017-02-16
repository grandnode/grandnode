using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Media;
using Grand.Services.Configuration;
using Grand.Services.Events;
using Grand.Services.Logging;
using Grand.Services.Seo;
using ImageProcessor;
using ImageProcessor.Imaging;

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
            MediaSettings mediaSettings)
        {
            this._pictureRepository = pictureRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._logger = logger;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
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
        /// <returns></returns>
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

            //also see System.Web.MimeMapping for more mime types

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
            var thumbDirectoryPath = CommonHelper.MapPath("~/content/images/thumbs");
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
            var thumbsDirectoryPath = CommonHelper.MapPath("~/content/images/thumbs");
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
        /// <param name="imagesDirectoryPath">Directory path with images; if null, then default one is used</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetPictureLocalPath(string fileName)
        {
            return Path.Combine(CommonHelper.MapPath("~/content/images/"), fileName);
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
                string path = CommonHelper.MapPath("~/Content/Images/Thumbs/");
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

                using (var mutex = new System.Threading.Mutex(false, thumbFileName))
                {
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        mutex.WaitOne();

                        if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                        {
                            using (var b = new Bitmap(filePath))
                            {
                                var pictureBinary = File.ReadAllBytes(filePath);

                                pictureBinary = ApplyResize(pictureBinary, targetSize, b.Size);
                                b.Dispose();

                                //UX these 2 pictureBinar'ies aren't the same byte array, assignation is needed
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
            lock (s_lock)
            {
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
                                            Bitmap b = null;
                                            try
                                            {
                                                //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                                b = new Bitmap(stream);
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

                                            //UX input pictureBinary isn't the same output pictureBinary (different MemoryStreams)
                                            pictureBinary = ApplyWatermark(pictureBinary);
                                        }
                                    }
                                    SaveThumb(thumbFilePath, thumbFileName, pictureBinary);
                                }
                            }
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
                                    //requested Picture with this size doesn't exist, generate new 
                                    using (var stream = new MemoryStream(pictureBinary))
                                    {
                                        Bitmap b = null;
                                        try
                                        {
                                            //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                            b = new Bitmap(stream);
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

                                        pictureBinary = ApplyResize(pictureBinary, targetSize, b.Size);
                                        b.Dispose();

                                        //UX these 2 pictureBinar'ies aren't the same byte array, assignation is needed
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
        /// Clears only physical Picture files located at ~/Content/Images/Thumbs/, it won't affect Pictures stored in database
        /// </summary>
        public void ClearThumbs()
        {
            const string searchPattern = "*.*";
            string path = CommonHelper.MapPath("~/Content/Images/Thumbs/");

            if (!System.IO.Directory.Exists(path))
                return;
            foreach (string str in System.IO.Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories))
            {
                if (str.Contains("pingo.exe") || str.Contains("placeholder.txt"))
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
        public virtual byte[] ValidatePicture(byte[] pictureBinary, string mimeType)
        {
            pictureBinary = ApplyResize(pictureBinary, _mediaSettings.MaximumImageSize);
            return pictureBinary;
        }


        protected byte[] ApplyResize(byte[] pictureBinary, int targetSize, Size originalSize = default(Size))
        {
            var resizeLayer = default(ResizeLayer);
            if (originalSize != default(Size))
            {
                resizeLayer = new ResizeLayer(CalculateDimensions(originalSize, targetSize), ResizeMode.Max);
            }
            else
            {
                resizeLayer = new ResizeLayer(new Size(targetSize, targetSize), ResizeMode.Max);
            }

            using (MemoryStream inStream = new MemoryStream(pictureBinary))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory())
                    {
                        imageFactory.Load(inStream)
                            .Resize(resizeLayer)
                            .Quality(_mediaSettings.DefaultImageQuality)
                            .Save(outStream);
                        inStream.Dispose();
                        imageFactory.Dispose();
                        return outStream.ToArray();
                    }
                }
            }
        }

        protected byte[] ApplyWatermark(byte[] pictureBinary)
        {
            //no Watermark Text and Watermark Overlay? Return
            if (string.IsNullOrEmpty(_mediaSettings.WatermarkText)
                && string.IsNullOrEmpty(_mediaSettings.WatermarkOverlayID)
                && _mediaSettings.WatermarkOverlayID == "0")
                return pictureBinary;

            //UX this piece of code needs to be here, otherwise "Input stream is not a supported format" Exception
            var pictureWidth = default(int);
            var pictureHeight = default(int);
            using (MemoryStream pictureStream = new MemoryStream(pictureBinary))
            {
                Bitmap bitmap = new Bitmap(Image.FromStream(pictureStream));
                pictureWidth = bitmap.Size.Width;
                pictureHeight = bitmap.Size.Height;
                pictureStream.Dispose();
            }

            using (MemoryStream inStream = new MemoryStream(pictureBinary))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory())
                    {
                        imageFactory
                            .Load(inStream);

                        if (!string.IsNullOrEmpty(_mediaSettings.WatermarkText))
                        {
                            var calculatedHorizontalPixel = (pictureWidth * _mediaSettings.WatermarkPositionXPercent) / 100;
                            var calculatedVerticalPixel = (pictureHeight * _mediaSettings.WatermarkPositionYPercent) / 100;
                            var calculatedFontSize = (pictureHeight * _mediaSettings.WatermarkFontSizePercent) / 100;

                            imageFactory
                                .Watermark(new TextLayer()
                                {
                                    Text = _mediaSettings.WatermarkText,
                                    Style = _mediaSettings.WatermarkStyle,
                                    FontColor = _mediaSettings.WatermarkFontColor,
                                    FontFamily = new FontFamily(_mediaSettings.WatermarkFontFamily),
                                    Opacity = _mediaSettings.WatermarkOpacityPercent,
                                    DropShadow = _mediaSettings.WatermarkDropShadow,
                                    Vertical = _mediaSettings.WatermarkVertical,
                                    Position = new Point(calculatedHorizontalPixel, calculatedVerticalPixel),
                                    RightToLeft = _mediaSettings.WatermarkRightToLeft,
                                    FontSize = calculatedFontSize <= 0 ? 1 : calculatedFontSize
                                });
                        }

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
                            using (var overlayStream = new MemoryStream(overlayByteArray))
                            {
                                imageFactory
                                    .Overlay(new ImageLayer()
                                    {
                                        Image = Image.FromStream(overlayStream),
                                        Opacity = _mediaSettings.WatermarkOverlayOpacityPercent,
                                        Position = new Point(overlayHorizontalPosition, overlayVerticalPosition),
                                        Size = new Size(overlayWidth, overlayHeight)
                                    });
                                overlayStream.Dispose();
                            }
                        }

                        imageFactory
                            .Save(outStream);

                        inStream.Dispose();
                        imageFactory.Dispose();
                        return outStream.ToArray();
                    }
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
                //var originalProxyCreationEnabled = _dbContext.ProxyCreationEnabled;
                try
                {
                    //we set this property for performance optimization
                    //it could be critical if you we have several thousand pictures
                    //_dbContext.ProxyCreationEnabled = false;

                    while (true)
                    {
                        var pictures = this.GetPictures(pageIndex, pageSize);
                        pageIndex++;

                        //all pictures converted?
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
                            //update appropriate properties
                            picture.PictureBinary = value ? pictureBinary : new byte[0];
                            picture.IsNew = true;
                            //raise event?
                            //_eventPublisher.EntityUpdated(picture);
                        }
                        //save all at once
                        _pictureRepository.Update(pictures);
                        //detach them in order to release memory
                    }
                }
                finally
                {
                    //_dbContext.ProxyCreationEnabled = originalProxyCreationEnabled;
                }
            }
        }

        #endregion
    }
}
