using Grand.Domain.Media;
using Grand.Framework.Security.Authorization;
using Grand.Services.Media;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Pictures)]
    public partial class PictureController : BaseAdminController
    {
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        public PictureController(IPictureService pictureService, MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        [NonAction]
        protected virtual IList<string> GetAllowedFileTypes()
        {
            if (string.IsNullOrEmpty(_mediaSettings.AllowedFileTypes))
                return new List<string> { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".webp" };
            else
                return _mediaSettings.AllowedFileTypes.Split(',');
        }

        [HttpPost]
        //do not validate request token (XSRF)
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AsyncUpload()
        {
            var form = await HttpContext.Request.ReadFormAsync();
            var httpPostedFile = form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty,
                });
            }

            
            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (String.IsNullOrEmpty(fileName) && form.ContainsKey(qqFileNameParameter))
                fileName = form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (!GetAllowedFileTypes().Contains(fileExtension))
            {
                return Json(new
                {
                    success = false,
                    pictureId = "",
                    imageUrl = ""
                });
            }

            //contentType is not always available 
            //that's why we manually update it here
            //http://www.sfsu.edu/training/mimetype.htm
            if (String.IsNullOrEmpty(contentType))
            {
                switch (fileExtension)
                {
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".tiff":
                    case ".tif":
                        contentType = "image/tiff";
                        break;
                    default:
                        break;
                }
            }

            var fileBinary = httpPostedFile.GetDownloadBits();
            var picture = await _pictureService.InsertPicture(fileBinary, contentType, null);
            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                pictureId = picture.Id,
                imageUrl = await _pictureService.GetPictureUrl(picture, 100)
            });
        }
    }
}
