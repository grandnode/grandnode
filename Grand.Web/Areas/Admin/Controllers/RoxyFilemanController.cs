﻿using Grand.Core;
using Grand.Framework.Security.Authorization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grand.Web.Areas.Admin.Extensions;

namespace Grand.Web.Areas.Admin.Controllers
{
    //Controller for Roxy fileman (http://www.roxyfileman.com/) for TinyMCE editor
    //the original file was \RoxyFileman-1.4.5-net\fileman\asp_net\main.ashx

    //do not validate request token (XSRF)
    [PermissionAuthorize(PermissionSystemName.Files)]
    public class RoxyFilemanController : BaseAdminController
    {
        #region Constants

        /// <summary>
        /// Default path to root directory of uploaded files (if appropriate settings are not specified)
        /// </summary>
        private const string DEFAULT_ROOT_DIRECTORY = "/content/images/uploaded";

        /// <summary>
        /// Path to directory of language files
        /// </summary>
        private const string LANGUAGE_DIRECTORY = "/administration/roxy_fileman/lang";

        /// <summary>
        /// Path to configuration file
        /// </summary>
        private const string CONFIGURATION_FILE = "/administration/roxy_fileman/conf.json";

        #endregion

        #region Fields

        private Dictionary<string, string> _settings;
        private Dictionary<string, string> _languageResources;

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public RoxyFilemanController(IWebHostEnvironment hostingEnvironment,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create configuration file for RoxyFileman
        /// </summary>
        public virtual void CreateConfiguration()
        {
            var filePath = GetFullPath(CONFIGURATION_FILE);

            //create file if not exists
            if (!System.IO.File.Exists(filePath))
            {
                //we use 'using' to close the file after it's created
                using (System.IO.File.Create(filePath)) { }
            }

            //try to read existing configuration
            var existingText = System.IO.File.ReadAllText(filePath);
            var existingConfiguration = JsonConvert.DeserializeAnonymousType(existingText, new
            {
                FILES_ROOT = string.Empty,
                SESSION_PATH_KEY = string.Empty,
                THUMBS_VIEW_WIDTH = string.Empty,
                THUMBS_VIEW_HEIGHT = string.Empty,
                PREVIEW_THUMB_WIDTH = string.Empty,
                PREVIEW_THUMB_HEIGHT = string.Empty,
                MAX_IMAGE_WIDTH = string.Empty,
                MAX_IMAGE_HEIGHT = string.Empty,
                DEFAULTVIEW = string.Empty,
                FORBIDDEN_UPLOADS = string.Empty,
                ALLOWED_UPLOADS = string.Empty,
                FILEPERMISSIONS = string.Empty,
                DIRPERMISSIONS = string.Empty,
                LANG = string.Empty,
                DATEFORMAT = string.Empty,
                OPEN_LAST_DIR = string.Empty,
                INTEGRATION = string.Empty,
                RETURN_URL_PREFIX = string.Empty,
                DIRLIST = string.Empty,
                CREATEDIR = string.Empty,
                DELETEDIR = string.Empty,
                MOVEDIR = string.Empty,
                COPYDIR = string.Empty,
                RENAMEDIR = string.Empty,
                FILESLIST = string.Empty,
                UPLOAD = string.Empty,
                DOWNLOAD = string.Empty,
                DOWNLOADDIR = string.Empty,
                DELETEFILE = string.Empty,
                MOVEFILE = string.Empty,
                COPYFILE = string.Empty,
                RENAMEFILE = string.Empty,
                GENERATETHUMB = string.Empty,
            });

            //check whether the path base has changed, otherwise there is no need to overwrite the configuration file
            var currentPathBase = this.HttpContext.Request.PathBase.ToString();
            if (existingConfiguration?.RETURN_URL_PREFIX?.Equals(currentPathBase) ?? false)
                return;

            //create configuration
            var configuration = new
            {
                FILES_ROOT = existingConfiguration?.FILES_ROOT ?? "content/images/uploaded",
                SESSION_PATH_KEY = existingConfiguration?.SESSION_PATH_KEY ?? string.Empty,
                THUMBS_VIEW_WIDTH = existingConfiguration?.THUMBS_VIEW_WIDTH ?? "140",
                THUMBS_VIEW_HEIGHT = existingConfiguration?.THUMBS_VIEW_HEIGHT ?? "120",
                PREVIEW_THUMB_WIDTH = existingConfiguration?.PREVIEW_THUMB_WIDTH ?? "300",
                PREVIEW_THUMB_HEIGHT = existingConfiguration?.PREVIEW_THUMB_HEIGHT ?? "200",
                MAX_IMAGE_WIDTH = existingConfiguration?.MAX_IMAGE_WIDTH ?? "1000",
                MAX_IMAGE_HEIGHT = existingConfiguration?.MAX_IMAGE_HEIGHT ?? "1000",
                DEFAULTVIEW = existingConfiguration?.DEFAULTVIEW ?? "list",
                FORBIDDEN_UPLOADS = existingConfiguration?.FORBIDDEN_UPLOADS ?? @"zip js jsp jsb mhtml mht xhtml xht php phtml 
                    php3 php4 php5 phps shtml jhtml pl sh py cgi exe application gadget hta cpl msc jar vb jse ws wsf wsc wsh 
                    ps1 ps2 psc1 psc2 msh msh1 msh2 inf reg scf msp scr dll msi vbs bat com pif cmd vxd cpl htpasswd htaccess",
                ALLOWED_UPLOADS = existingConfiguration?.ALLOWED_UPLOADS ?? string.Empty,
                FILEPERMISSIONS = existingConfiguration?.FILEPERMISSIONS ?? "0644",
                DIRPERMISSIONS = existingConfiguration?.DIRPERMISSIONS ?? "0755",
                LANG = existingConfiguration?.LANG ?? _workContext.WorkingLanguage.UniqueSeoCode,
                DATEFORMAT = existingConfiguration?.DATEFORMAT ?? "dd/MM/yyyy HH:mm",
                OPEN_LAST_DIR = existingConfiguration?.OPEN_LAST_DIR ?? "yes",

                //no need user to configure
                INTEGRATION = "tinymce4",
                RETURN_URL_PREFIX = currentPathBase,
                DIRLIST = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=DIRLIST",
                CREATEDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=CREATEDIR",
                DELETEDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=DELETEDIR",
                MOVEDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=MOVEDIR",
                COPYDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=COPYDIR",
                RENAMEDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=RENAMEDIR",
                FILESLIST = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=FILESLIST",
                UPLOAD = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=UPLOAD",
                DOWNLOAD = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=DOWNLOAD",
                DOWNLOADDIR = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=DOWNLOADDIR",
                DELETEFILE = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=DELETEFILE",
                MOVEFILE = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=MOVEFILE",
                COPYFILE = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=COPYFILE",
                RENAMEFILE = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=RENAMEFILE",
                GENERATETHUMB = $"{HttpContext.Request.PathBase}/{Constants.AreaAdmin}/RoxyFileman/ProcessRequest?a=GENERATETHUMB",
            };

            //save the file
            var text = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, text);
        }

        /// <summary>
        /// Process request
        /// </summary>
        [IgnoreAntiforgeryToken]
        public virtual async Task ProcessRequest()
        {
            //async requests are disabled in the js code, so use .Wait() method here
            await ProcessRequestAsync();
        }

        #endregion

        #region Utitlies

        /// <summary>
        /// Process the incoming request
        /// </summary>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task ProcessRequestAsync()
        {
            var action = "DIRLIST";
            try
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.HtmlEditorManagePictures))
                    throw new Exception("You don't have required permission");

                if (!StringValues.IsNullOrEmpty(HttpContext.Request.Query["a"]))
                    action = HttpContext.Request.Query["a"];

                switch (action.ToUpper())
                {
                    case "DIRLIST":
                        await GetDirectoriesAsync(HttpContext.Request.Query["type"]);
                        break;
                    case "FILESLIST":
                        await GetFilesAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["type"]);
                        break;
                    case "COPYDIR":
                        await CopyDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                        break;
                    case "COPYFILE":
                        await CopyFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                        break;
                    case "CREATEDIR":
                        await CreateDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                        break;
                    case "DELETEDIR":
                        await DeleteDirectoryAsync(HttpContext.Request.Query["d"]);
                        break;
                    case "DELETEFILE":
                        await DeleteFileAsync(HttpContext.Request.Query["f"]);
                        break;
                    case "DOWNLOAD":
                        await DownloadFileAsync(HttpContext.Request.Query["f"]);
                        break;
                    case "DOWNLOADDIR":
                        await DownloadDirectoryAsync(HttpContext.Request.Query["d"]);
                        break;
                    case "MOVEDIR":
                        await MoveDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                        break;
                    case "MOVEFILE":
                        await MoveFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                        break;
                    case "RENAMEDIR":
                        await RenameDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                        break;
                    case "RENAMEFILE":
                        await RenameFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                        break;
                    case "GENERATETHUMB":
                        await CreateThumbnail(HttpContext.Request.Query["f"]);
                        break;
                    case "UPLOAD":
                        var form = await HttpContext.Request.ReadFormAsync();
                        await UploadFilesAsync(form["d"]);
                        break;
                    default:
                        await HttpContext.Response.WriteAsync(GetErrorResponse("This action is not implemented."));
                        break;
                }
            }
            catch (Exception ex)
            {
                if (action == "UPLOAD" && !await IsAjaxRequest())
                    await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({GetErrorResponse(GetLanguageResource("E_UploadNoFiles"))});</script>");
                else
                    await HttpContext.Response.WriteAsync(GetErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get the virtual path to root directory of uploaded files 
        /// </summary>
        /// <returns>Path</returns>
        protected virtual string GetRootDirectory()
        {
            var filesRoot = GetSetting("FILES_ROOT");

            var sessionPathKey = GetSetting("SESSION_PATH_KEY");
            if (!string.IsNullOrEmpty(sessionPathKey))
                filesRoot = HttpContext.Session.GetString(sessionPathKey);

            if (string.IsNullOrEmpty(filesRoot))
                filesRoot = DEFAULT_ROOT_DIRECTORY;

            return filesRoot;
        }

        /// <summary>
        /// Get a virtual path with the root directory
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Path</returns>
        protected virtual string GetVirtualPath(string path)
        {
            var rootDirectory = GetRootDirectory();
            path = path ?? rootDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                var fullPath = System.IO.Path.GetFullPath(path);
                var fullPathRootDirectory = System.IO.Path.GetFullPath(rootDirectory);
                if (!fullPath.StartsWith(fullPathRootDirectory))
                    path = rootDirectory;
            }

            return path;
        }

        /// <summary>
        /// Get the absolute path by virtual path
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <returns>Path</returns>
        protected virtual string GetFullPath(string virtualPath)
        {
            virtualPath = virtualPath ?? string.Empty;
            if (!virtualPath.StartsWith("/"))
                virtualPath = "/" + virtualPath;
            virtualPath = virtualPath.TrimEnd('/');
            if (Grand.Core.OperatingSystem.IsWindows())
                virtualPath = virtualPath.Replace('/', '\\');

            return _hostingEnvironment.WebRootPath + virtualPath;
        }

        /// <summary>
        /// Get a value of the configuration setting
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <returns>Setting value</returns>
        protected virtual string GetSetting(string key)
        {
            if (_settings == null)
                _settings = ParseJson(GetFullPath(CONFIGURATION_FILE));

            if (_settings.TryGetValue(key, out string value))
                return value;

            return null;
        }

        /// <summary>
        /// Get the language resource value
        /// </summary>
        /// <param name="key">Language resource key</param>
        /// <returns>Language resource value</returns>
        protected virtual string GetLanguageResource(string key)
        {
            if (_languageResources == null)
                _languageResources = ParseJson(GetLanguageFile());

            if (_languageResources.TryGetValue(key, out string value))
                return value;

            return key;
        }

        /// <summary>
        /// Get the absolute path to the language resources file
        /// </summary>
        /// <returns>Path</returns>
        protected virtual string GetLanguageFile()
        {
            var languageCode = GetSetting("LANG");
            var languageFile = $"{LANGUAGE_DIRECTORY}/{languageCode}.json";

            if (!System.IO.File.Exists(GetFullPath(languageFile)))
                languageFile = $"{LANGUAGE_DIRECTORY}/en.json";

            return GetFullPath(languageFile);
        }

        /// <summary>
        /// Parse the JSON file
        /// </summary>
        /// <param name="file">Path to the file</param>
        /// <returns>Collection of keys and values from the parsed file</returns>
        protected virtual Dictionary<string, string> ParseJson(string file)
        {
            var result = new Dictionary<string, string>();
            var json = string.Empty;
            try
            {
                json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8)?.Trim();
            }
            catch { }

            if (string.IsNullOrEmpty(json))
                return result;

            if (json.StartsWith("{"))
                json = json.Substring(1, json.Length - 2);

            json = json.Trim();
            json = json.Substring(1, json.Length - 2);

            var lines = Regex.Split(json, "\"\\s*,\\s*\"");
            foreach (var line in lines)
            {
                var tmp = Regex.Split(line, "\"\\s*:\\s*\"");
                try
                {
                    if (!string.IsNullOrEmpty(tmp[0]) && !result.ContainsKey(tmp[0]))
                        result.Add(tmp[0], tmp[1]);
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Get a file type by file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <returns>File type</returns>
        protected virtual string GetFileType(string fileExtension)
        {
            var fileType = "file";

            fileExtension = fileExtension.ToLower();
            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif" || fileExtension == ".bmp" || fileExtension == ".webp")
                fileType = "image";

            if (fileExtension == ".swf" || fileExtension == ".flv")
                fileType = "flash";

            if (fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mov")
                fileType = "media";

            return fileType;
        }

        /// <summary>
        /// Check whether there are any restrictions on handling the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>True if the file can be handled; otherwise false</returns>
        protected virtual bool CanHandleFile(string path)
        {
            var result = false;
            var fileExtension = new FileInfo(path).Extension.Replace(".", "").ToLower();

            var forbiddenUploads = GetSetting("FORBIDDEN_UPLOADS").Trim().ToLower();
            if (!string.IsNullOrEmpty(forbiddenUploads))
            {
                var forbiddenFileExtensions = new ArrayList(Regex.Split(forbiddenUploads, "\\s+"));
                result = !forbiddenFileExtensions.Contains(fileExtension);
            }

            var allowedUploads = GetSetting("ALLOWED_UPLOADS").Trim().ToLower();
            if (!string.IsNullOrEmpty(allowedUploads))
            {
                var allowedFileExtensions = new ArrayList(Regex.Split(allowedUploads, "\\s+"));
                result = allowedFileExtensions.Contains(fileExtension);
            }

            return result;
        }

        /// <summary>
        /// Get the string to write to the response
        /// </summary>
        /// <param name="type">Type of the response</param>
        /// <param name="message">Additional message</param>
        /// <returns>String to write to the response</returns>
        protected virtual string GetResponse(string type, string message)
        {
            return $"{{\"res\":\"{type}\",\"msg\":\"{message?.Replace("\"", "\\\"")}\"}}";
        }

        /// <summary>
        /// Get the string to write a success response
        /// </summary>
        /// <param name="message">Additional message</param>
        /// <returns>String to write to the response</returns>
        protected virtual string GetSuccessResponse(string message = null)
        {
            return GetResponse("ok", message);
        }

        /// <summary>
        /// Get the string to write an error response
        /// </summary>
        /// <param name="message">Additional message</param>
        /// <returns>String to write to the response</returns>
        protected virtual string GetErrorResponse(string message = null)
        {
            return GetResponse("error", message);
        }

        /// <summary>
        /// Get all available directories as a directory tree
        /// </summary>
        /// <param name="type">Type of the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task GetDirectoriesAsync(string type)
        {
            var rootDirectoryPath = GetFullPath(GetVirtualPath(null));
            var rootDirectory = new DirectoryInfo(rootDirectoryPath);
            if (!rootDirectory.Exists)
                throw new Exception("Invalid files root directory. Check your configuration - " + rootDirectoryPath);

            var allDirectories = GetDirectories(rootDirectory.FullName);
            allDirectories.Insert(0, rootDirectory.FullName);

            var localPath = GetFullPath(null);
            await HttpContext.Response.WriteAsync("[");
            for (var i = 0; i < allDirectories.Count; i++)
            {
                var directoryPath = (string)allDirectories[i];
                await HttpContext.Response.WriteAsync($"{{\"p\":\"/{directoryPath.Replace(localPath, string.Empty).Replace("\\", "/").TrimStart('/')}\",\"f\":\"{GetFiles(directoryPath, type).Count}\",\"d\":\"{Directory.GetDirectories(directoryPath).Length}\"}}");
                if (i < allDirectories.Count - 1)
                    await HttpContext.Response.WriteAsync(",");
            }
            await HttpContext.Response.WriteAsync("]");
        }

        /// <summary>
        /// Get directories in the passed parent directory
        /// </summary>
        /// <param name="parentDirectoryPath">Path to the parent directory</param>
        /// <returns>Array of the paths to the directories</returns>
        protected virtual ArrayList GetDirectories(string parentDirectoryPath)
        {
            var directories = new ArrayList();

            var directoryNames = Directory.GetDirectories(parentDirectoryPath);
            foreach (var directory in directoryNames)
            {
                directories.Add(directory);
                directories.AddRange(GetDirectories(directory));
            }

            return directories;
        }

        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task GetFilesAsync(string directoryPath, string type)
        {
            directoryPath = GetVirtualPath(directoryPath);
            var files = GetFiles(GetFullPath(directoryPath), type);

            await HttpContext.Response.WriteAsync("[");
            for (var i = 0; i < files.Count; i++)
            {
                var width = 0;
                var height = 0;
                var file = new FileInfo(files[i]);
                if (GetFileType(file.Extension) == "image")
                {
                    try
                    {
                        using (var stream = new FileStream(file.FullName, FileMode.Open))
                        {
                            using (var image = SKBitmap.Decode(stream))
                            {
                                width = image.Width;
                                height = image.Height;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                await HttpContext.Response.WriteAsync($"{{\"p\":\"{directoryPath.TrimEnd('/')}/{file.Name}\",\"t\":\"{Math.Ceiling(GetTimestamp(file.LastWriteTime))}\",\"s\":\"{file.Length}\",\"w\":\"{width}\",\"h\":\"{height}\"}}");

                if (i < files.Count - 1)
                    await HttpContext.Response.WriteAsync(",");
            }
            await HttpContext.Response.WriteAsync("]");
        }

        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>List of paths to the files</returns>
        protected virtual List<string> GetFiles(string directoryPath, string type)
        {
            if (type == "#")
                type = string.Empty;

            var files = new List<string>();
            foreach (var fileName in Directory.GetFiles(directoryPath))
            {
                if (string.IsNullOrEmpty(type) || GetFileType(new FileInfo(fileName).Extension) == type)
                    files.Add(fileName);
            }

            return files;
        }

        /// <summary>
        /// Get the Unix timestamp by passed date
        /// </summary>
        /// <param name="date">Date and time</param>
        /// <returns>Unix timestamp</returns>
        protected virtual double GetTimestamp(DateTime date)
        {
            return (date.ToLocalTime() - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }

        /// <summary>
        /// Copy the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task CopyDirectoryAsync(string sourcePath, string destinationPath)
        {
            var directoryPath = GetFullPath(GetVirtualPath(sourcePath));
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
                throw new Exception(GetLanguageResource("E_CopyDirInvalidPath"));

            var newDirectoryPath = GetFullPath(GetVirtualPath($"{destinationPath.TrimEnd('/')}/{directory.Name}"));
            var newDirectory = new DirectoryInfo(newDirectoryPath);
            if (newDirectory.Exists)
                throw new Exception(GetLanguageResource("E_DirAlreadyExists"));

            CopyDirectory(directory.FullName, newDirectory.FullName);

            await HttpContext.Response.WriteAsync(GetSuccessResponse());
        }

        /// <summary>
        /// Сopy the directory with the embedded files and directories
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        protected virtual void CopyDirectory(string sourcePath, string destinationPath)
        {
            var existingFiles = Directory.GetFiles(sourcePath);
            var existingDirectories = Directory.GetDirectories(sourcePath);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (var file in existingFiles)
            {
                var filePath = Path.Combine(destinationPath, new FileInfo(file).Name);
                if (!System.IO.File.Exists(filePath))
                    System.IO.File.Copy(file, filePath);
            }

            foreach (var directory in existingDirectories)
            {
                var directoryPath = Path.Combine(destinationPath, new DirectoryInfo(directory).Name);
                CopyDirectory(directory, directoryPath);
            }
        }

        /// <summary>
        /// Copy the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            var filePath = GetFullPath(GetVirtualPath(sourcePath));
            var file = new FileInfo(filePath);
            if (!file.Exists)
                throw new Exception(GetLanguageResource("E_CopyFileInvalisPath"));

            destinationPath = GetFullPath(GetVirtualPath(destinationPath));
            var newFileName = GetUniqueFileName(destinationPath, file.Name);
            try
            {
                System.IO.File.Copy(file.FullName, Path.Combine(destinationPath, newFileName));
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception(GetLanguageResource("E_CopyFile"));
            }
        }

        /// <summary>
        /// Get the unique name of the file (add -copy-(N) to the file name if there is already a file with that name in the directory)
        /// </summary>
        /// <param name="directoryPath">Path to the file directory</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Unique name of the file</returns>
        protected virtual string GetUniqueFileName(string directoryPath, string fileName)
        {
            var uniqueFileName = fileName;

            var i = 0;
            while (System.IO.File.Exists(Path.Combine(directoryPath, uniqueFileName)))
            {
                uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-Copy-{++i}{Path.GetExtension(fileName)}";
            }

            return uniqueFileName;
        }

        /// <summary>
        /// Create the new directory
        /// </summary>
        /// <param name="parentDirectoryPath">Path to the parent directory</param>
        /// <param name="name">Name of the new directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task CreateDirectoryAsync(string parentDirectoryPath, string name)
        {
            parentDirectoryPath = GetFullPath(GetVirtualPath(parentDirectoryPath));
            if (!Directory.Exists(parentDirectoryPath))
                throw new Exception(GetLanguageResource("E_CreateDirInvalidPath"));

            try
            {
                var path = Path.Combine(parentDirectoryPath, name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception(GetLanguageResource("E_CreateDirFailed"));
            }
        }

        /// <summary>
        /// Delete the directory
        /// </summary>
        /// <param name="path">Path to the directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task DeleteDirectoryAsync(string path)
        {
            path = GetVirtualPath(path);
            if (path == GetRootDirectory())
                throw new Exception(GetLanguageResource("E_CannotDeleteRoot"));

            path = GetFullPath(path);
            if (!Directory.Exists(path))
                throw new Exception(GetLanguageResource("E_DeleteDirInvalidPath"));

            if (Directory.GetDirectories(path).Length > 0 || Directory.GetFiles(path).Length > 0)
                throw new Exception(GetLanguageResource("E_DeleteNonEmpty"));

            try
            {
                Directory.Delete(path);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception(GetLanguageResource("E_CannotDeleteDir"));
            }
        }

        /// <summary>
        /// Delete the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task DeleteFileAsync(string path)
        {
            path = GetFullPath(GetVirtualPath(path));
            if (!System.IO.File.Exists(path))
                throw new Exception(GetLanguageResource("E_DeleteFileInvalidPath"));

            try
            {
                System.IO.File.Delete(path);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception(GetLanguageResource("E_DeletеFile"));
            }
        }

        /// <summary>
        /// Download the directory from the server as a zip archive
        /// </summary>
        /// <param name="path">Path to the directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        public async Task DownloadDirectoryAsync(string path)
        {
            path = GetVirtualPath(path).TrimEnd('/');
            var fullPath = GetFullPath(path);
            if (!Directory.Exists(fullPath))
                throw new Exception(GetLanguageResource("E_CreateArchive"));

            var zipName = new FileInfo(fullPath).Name + ".zip";
            var zipPath = $"/{zipName}";
            if (path != GetRootDirectory())
                zipPath = GetVirtualPath(zipPath);
            zipPath = GetFullPath(zipPath);

            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(fullPath, zipPath, CompressionLevel.Fastest, true);

            HttpContext.Response.Clear();
            HttpContext.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{zipName}\"");
            HttpContext.Response.ContentType = "application/force-download";
            await HttpContext.Response.SendFileAsync(zipPath);

            System.IO.File.Delete(zipPath);
        }

        /// <summary>
        /// Download the file from the server
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task DownloadFileAsync(string path)
        {
            var filePath = GetFullPath(GetVirtualPath(path));
            var file = new FileInfo(filePath);
            if (file.Exists)
            {
                HttpContext.Response.Clear();
                HttpContext.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                HttpContext.Response.ContentType = "application/force-download";
                await HttpContext.Response.SendFileAsync(file.FullName);
            }
        }

        /// <summary>
        /// Move the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task MoveDirectoryAsync(string sourcePath, string destinationPath)
        {
            var fullSourcePath = GetFullPath(GetVirtualPath(sourcePath));
            var sourceDirectory = new DirectoryInfo(fullSourcePath);
            destinationPath = GetFullPath(GetVirtualPath(Path.Combine(destinationPath, sourceDirectory.Name)));
            var destinationDirectory = new DirectoryInfo(destinationPath);
            if (destinationDirectory.FullName.IndexOf(sourceDirectory.FullName) == 0)
                throw new Exception(GetLanguageResource("E_CannotMoveDirToChild"));

            if (!sourceDirectory.Exists)
                throw new Exception(GetLanguageResource("E_MoveDirInvalisPath"));

            if (destinationDirectory.Exists)
                throw new Exception(GetLanguageResource("E_DirAlreadyExists"));

            try
            {
                sourceDirectory.MoveTo(destinationDirectory.FullName);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception($"{GetLanguageResource("E_MoveDir")} \"{sourcePath}\"");
            }
        }

        /// <summary>
        /// Move the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            var fullSourcePath = GetFullPath(GetVirtualPath(sourcePath));
            var sourceFile = new FileInfo(fullSourcePath);
            if (!sourceFile.Exists)
                throw new Exception(GetLanguageResource("E_MoveFileInvalisPath"));

            destinationPath = GetFullPath(GetVirtualPath(destinationPath));
            var destinationFile = new FileInfo(destinationPath);
            if (destinationFile.Exists)
                throw new Exception(GetLanguageResource("E_MoveFileAlreadyExists"));

            if (!CanHandleFile(destinationFile.Name))
                throw new Exception(GetLanguageResource("E_FileExtensionForbidden"));

            try
            {
                sourceFile.MoveTo(destinationFile.FullName);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception($"{GetLanguageResource("E_MoveFile")} \"{sourcePath}\"");
            }
        }

        /// <summary>
        /// Rename the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="newName">New name of the directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task RenameDirectoryAsync(string sourcePath, string newName)
        {
            var fullSourcePath = GetFullPath(GetVirtualPath(sourcePath));
            var sourceDirectory = new DirectoryInfo(fullSourcePath);
            var destinationDirectory = new DirectoryInfo(Path.Combine(sourceDirectory.Parent.FullName, newName));
            if (GetVirtualPath(sourcePath) == GetRootDirectory())
                throw new Exception(GetLanguageResource("E_CannotRenameRoot"));

            if (!sourceDirectory.Exists)
                throw new Exception(GetLanguageResource("E_RenameDirInvalidPath"));

            if (destinationDirectory.Exists)
                throw new Exception(GetLanguageResource("E_DirAlreadyExists"));

            try
            {
                sourceDirectory.MoveTo(destinationDirectory.FullName);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception($"{GetLanguageResource("E_RenameDir")} \"{sourcePath}\"");
            }
        }

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="newName">New name of the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task RenameFileAsync(string sourcePath, string newName)
        {
            var fullSourcePath = GetFullPath(GetVirtualPath(sourcePath));
            var sourceFile = new FileInfo(fullSourcePath);
            if (!sourceFile.Exists)
                throw new Exception(GetLanguageResource("E_RenameFileInvalidPath"));

            if (!CanHandleFile(newName))
                throw new Exception(GetLanguageResource("E_FileExtensionForbidden"));

            try
            {
                var destinationPath = Path.Combine(sourceFile.Directory.FullName, newName);
                var destinationFile = new FileInfo(destinationPath);
                sourceFile.MoveTo(destinationFile.FullName);
                await HttpContext.Response.WriteAsync(GetSuccessResponse());
            }
            catch
            {
                throw new Exception($"{GetLanguageResource("E_RenameFile")} \"{sourcePath}\"");
            }
        }

        /// <summary>
        /// Create the thumbnail of the image and write it to the response
        /// </summary>
        /// <param name="path">Path to the image</param>
        protected virtual async Task CreateThumbnail(string path)
        {
            try
            {
                path = GetFullPath(GetVirtualPath(path));
                HttpContext.Response.Headers.Add("Content-Type", "image/png");
                byte[] file = System.IO.File.ReadAllBytes(path);
                using (var image = SKBitmap.Decode(file))
                {
                    float width, height;
                    int targetSize = 120;
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
                    using (var resized = image.Resize(new SKImageInfo((int)width, (int)height), SKFilterQuality.None))
                    {
                        using (var image2 = SKImage.FromBitmap(resized))
                        {
                            file = image2.Encode().ToArray();
                        }
                    }
                }
                await HttpContext.Response.Body.WriteAsync(file, 0, file.Length);
                HttpContext.Response.Body.Close();

            }
            catch { }
        }

        /// <summary>
        /// Whether the request is made with ajax 
        /// </summary>
        /// <returns>True or false</returns>
        protected virtual async Task<bool> IsAjaxRequest()
        {
            var form = await HttpContext.Request.ReadFormAsync();
            return form != null &&
                !StringValues.IsNullOrEmpty(form["method"]) &&
                form["method"] == "ajax";
        }

        /// <summary>
        /// Upload files to a directory on passed path
        /// </summary>
        /// <param name="directoryPath">Path to directory to upload files</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task UploadFilesAsync(string directoryPath)
        {
            var result = GetSuccessResponse();
            var hasErrors = false;
            try
            {
                directoryPath = GetFullPath(GetVirtualPath(directoryPath));
                var form = await HttpContext.Request.ReadFormAsync();
                for (var i = 0; i < form.Files.Count; i++)
                {
                    var fileName = form.Files[i].FileName;
                    if (CanHandleFile(fileName))
                    {
                        var file = new FileInfo(fileName);
                        var uniqueFileName = GetUniqueFileName(directoryPath, file.Name);
                        var destinationFile = Path.Combine(directoryPath, uniqueFileName);
                        using (var stream = new FileStream(destinationFile, FileMode.OpenOrCreate))
                        {
                            form.Files[i].CopyTo(stream);
                        }
                    }
                    else
                    {
                        hasErrors = true;
                        result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));
                    }
                }
            }
            catch (Exception ex)
            {
                result = GetErrorResponse(ex.Message);
            }
            if (await IsAjaxRequest())
            {
                if (hasErrors)
                    result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));

                await HttpContext.Response.WriteAsync(result);
            }
            else
                await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({result});</script>");
        }

        #endregion
    }
}