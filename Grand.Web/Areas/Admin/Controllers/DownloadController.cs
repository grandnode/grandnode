﻿using Grand.Core.Domain.Media;
using Grand.Framework.Security;
using Grand.Framework.Security.Authorization;
using Grand.Services.Media;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Files)]
    public partial class DownloadController : BaseAdminController
    {
        private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            this._downloadService = downloadService;
        }

        public async Task<IActionResult> DownloadFile(Guid downloadGuid)
        {
            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
            if (download == null)
                return Content("No download record found with the specified id");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //use stored data
            if (download.DownloadBinary == null)
                return Content(string.Format("Download data is not available any more. Download GD={0}", download.Id));

            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
            string contentType = !String.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType)
            {
                FileDownloadName = fileName + download.Extension
            };
        }

        [HttpPost]
        
        //do not validate request token (XSRF)
        [IgnoreAntiforgeryToken] 
        public async Task<IActionResult> SaveDownloadUrl(string downloadUrl)
        {
            if(string.IsNullOrEmpty(downloadUrl))
            {
                return Json(new { success = false, error = "URL can't be empty" });
            }
            //insert
            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = true,
                DownloadUrl = downloadUrl,
                IsNew = true
              };
            await _downloadService.InsertDownload(download);

            return Json(new { downloadId = download.Id, success = true });
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

            var fileBinary = httpPostedFile.GetDownloadBits();

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


            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                downloadId = download.Id,
                downloadUrl = Url.Action("DownloadFile", new { downloadGuid = download.DownloadGuid })
            });
        }
    }
}