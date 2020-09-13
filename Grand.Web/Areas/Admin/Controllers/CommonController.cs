using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Plugins;
using Grand.Core.Roslyn;
using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Grand.Domain.Directory;
using Grand.Domain.Media;
using Grand.Domain.Seo;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security;
using Grand.Framework.Security.Authorization;
using Grand.Services.Commands.Models.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.MachineNameProvider;
using Grand.Services.Media;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Maintenance)]
    public partial class CommonController : BaseAdminController
    {
        #region Fields

        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IMachineNameProvider _machineNameProvider;
        private readonly IMediator _mediator;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly GrandConfig _grandConfig;
        #endregion

        #region Constructors

        public CommonController(IPaymentService paymentService,
            IShippingService shippingService,
            ICurrencyService currencyService,
            IMeasureService measureService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IStoreService storeService,
            IMongoDBContext mongoDBContext,
            IMachineNameProvider machineNameProvider,
            IMediator mediator,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            MeasureSettings measureSettings,
            GrandConfig grandConfig
            )
        {
            _paymentService = paymentService;
            _shippingService = shippingService;
            _currencyService = currencyService;
            _measureService = measureService;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _currencySettings = currencySettings;
            _measureSettings = measureSettings;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _storeService = storeService;
            _catalogSettings = catalogSettings;
            _grandConfig = grandConfig;
            _mongoDBContext = mongoDBContext;
            _machineNameProvider = machineNameProvider;
            _mediator = mediator;
        }

        #endregion

        protected virtual IEnumerable<Dictionary<string, object>> Serialize(List<BsonValue> collection)
        {
            var results = new List<Dictionary<string, object>>();
            var columns = new List<string>();
            var document = collection.FirstOrDefault()?.AsBsonDocument;
            var cols = new List<string>();
            if (document != null)
            {
                foreach (var item in document.Names)
                {
                    columns.Add(item);
                }
                foreach (var row in collection)
                {
                    var myObject = new Dictionary<string, object>();
                    foreach (var col in columns)
                    {
                        myObject.Add(col, row[col].ToString());
                    }
                    results.Add(myObject);
                }
            }
            return results;
        }

        #region Methods

        public IActionResult SystemInfo()
        {
            var model = new SystemInfoModel();
            model.GrandVersion = GrandVersion.FullVersion;
            try
            {
                model.OperatingSystem = RuntimeInformation.OSDescription;
            }
            catch (Exception) { }
            try
            {
                model.AspNetInfo = RuntimeEnvironment.GetSystemVersion();
            }
            catch (Exception) { }

            model.MachineName = _machineNameProvider.GetMachineName();

            model.ServerTimeZone = TimeZoneInfo.Local.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.ApplicationTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
            model.UtcTime = DateTime.UtcNow;
            foreach (var header in HttpContext.Request.Headers)
            {
                model.ServerVariables.Add(new SystemInfoModel.ServerVariableModel {
                    Name = header.Key,
                    Value = header.Value
                });
            }
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().ToList().OrderBy(x=>x.FullName))
            {
                model.LoadedAssemblies.Add(new SystemInfoModel.LoadedAssembly {
                    FullName = assembly.FullName,
                });
            }
            return View(model);
        }


        public async Task<IActionResult> Warnings()
        {
            var model = new List<SystemWarningModel>();

            //store URL
            var currentStoreUrl = _storeContext.CurrentStore.Url;
            if (!String.IsNullOrEmpty(currentStoreUrl) &&
                (currentStoreUrl.Equals(_webHelper.GetStoreLocation(false), StringComparison.OrdinalIgnoreCase)
                ||
                currentStoreUrl.Equals(_webHelper.GetStoreLocation(true), StringComparison.OrdinalIgnoreCase)
                ))
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.URL.Match")
                });
            else
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Warning,
                    Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.URL.NoMatch"), currentStoreUrl, _webHelper.GetStoreLocation(false))
                });


            //primary exchange rate currency
            var perCurrency = await _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
            if (perCurrency != null)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Set"),
                });
                if (perCurrency.Rate != 1)
                {
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Rate1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.NotSet")
                });
            }

            //primary store currency
            var pscCurrency = await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (pscCurrency != null)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.Set"),
                });
            }
            else
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.NotSet")
                });
            }


            //base measure weight
            var bWeight = await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (bWeight != null)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Set"),
                });

                if (bWeight.Ratio != 1)
                {
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.NotSet")
                });
            }


            //base dimension weight
            var bDimension = await _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (bDimension != null)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Set"),
                });

                if (bDimension.Ratio != 1)
                {
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.NotSet")
                });
            }

            //shipping rate coputation methods
            var srcMethods = await _shippingService.LoadActiveShippingRateComputationMethods();
            if (srcMethods.Count == 0)
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.NoComputationMethods")
                });
            if (srcMethods.Count(x => x.ShippingRateComputationMethodType == ShippingRateComputationMethodType.Offline) > 1)
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.OnlyOneOffline")
                });

            //payment methods
            if ((await _paymentService.LoadActivePaymentMethods()).Any())
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.OK")
                });
            else
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.NoActive")
                });

            //incompatible plugins
            if (PluginManager.IncompatiblePlugins != null)
                foreach (var pluginName in PluginManager.IncompatiblePlugins)
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.IncompatiblePlugin"), pluginName)
                    });

            //performance settings
            if (!_catalogSettings.IgnoreStoreLimitations && (await _storeService.GetAllStores()).Count == 1)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Performance.IgnoreStoreLimitations")
                });
            }
            if (!_catalogSettings.IgnoreAcl)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Performance.IgnoreAcl")
                });
            }

            //validate write permissions (the same procedure like during installation)
            var dirPermissionsOk = true;
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
            foreach (string dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                {
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.Wrong"), WindowsIdentity.GetCurrent().Name, dir)
                    });
                    dirPermissionsOk = false;
                }
            if (dirPermissionsOk)
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.OK")
                });

            var filePermissionsOk = true;
            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (string file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                {
                    model.Add(new SystemWarningModel {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.FilePermission.Wrong"), WindowsIdentity.GetCurrent().Name, file)
                    });
                    filePermissionsOk = false;
                }
            if (filePermissionsOk)
            {
                model.Add(new SystemWarningModel {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.FilePermission.OK")
                });
            }
            return View(model);
        }


        public IActionResult Maintenance()
        {
            var model = new MaintenanceModel();
            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;
            model.DeleteAbandonedCarts.OlderThan = DateTime.UtcNow.AddDays(-182);
            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-guests")]
        public async Task<IActionResult> MaintenanceDeleteGuests(MaintenanceModel model)
        {
            DateTime? startDateValue = (model.DeleteGuests.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteGuests.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            model.DeleteGuests.NumberOfDeletedCustomers = await _customerService.DeleteGuestCustomers(startDateValue, endDateValue, model.DeleteGuests.OnlyWithoutShoppingCart);

            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("clear-most-view")]
        public async Task<IActionResult> MaintenanceClearMostViewed(MaintenanceModel model)
        {
            await _mediator.Send(new ClearMostViewedCommand());
            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-exported-files")]
        public IActionResult MaintenanceDeleteFiles(MaintenanceModel model)
        {
            //TO DO
            model.DeleteExportedFiles.NumberOfDeletedFiles = 0;
            return View(model);
        }


        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-activitylog")]
        public async Task<IActionResult> MaintenanceDeleteActivitylog(MaintenanceModel model)
        {
            await _mediator.Send(new DeleteActivitylogCommand());
            model.DeleteActivityLog = true;
            return View(model);
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("convert-picture-webp")]
        public async Task<IActionResult> MaintenanceConvertPicture([FromServices] IPictureService pictureService, [FromServices] MediaSettings mediaSettings)
        {
            var model = new MaintenanceModel();
            model.ConvertedPictureModel.NumberOfConvertItems = 0;
            if (mediaSettings.StoreInDb)
            {
                var pictures = pictureService.GetPictures();
                foreach (var picture in pictures)
                {
                    if (!picture.MimeType.Contains("webp"))
                    {
                        using var image = SKBitmap.Decode(picture.PictureBinary);
                        SKData d = SKImage.FromBitmap(image).Encode(SKEncodedImageFormat.Webp, mediaSettings.DefaultImageQuality);
                        await pictureService.UpdatePicture(picture.Id, d.ToArray(), "image/webp", picture.SeoFilename, picture.AltAttribute, picture.TitleAttribute, true, false);
                        model.ConvertedPictureModel.NumberOfConvertItems += 1;
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ClearCache(string returnUrl, [FromServices] ICacheManager cacheManager)
        {
            await cacheManager.Clear();

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }


        public IActionResult RestartApplication(string returnUrl = "")
        {
            //restart application
            _webHelper.RestartAppDomain();

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }

        public IActionResult Roslyn()
        {
            return View(_grandConfig.UseRoslynScripts);
        }

        [HttpPost]
        public IActionResult Roslyn(DataSourceRequest command)
        {
            var scripts = RoslynCompiler.ReferencedScripts != null ? RoslynCompiler.ReferencedScripts.ToList() : new List<ResultCompiler>();

            var gridModel = new DataSourceResult {
                Data = scripts.Select(x =>
                {
                    return new
                    {
                        FileName = x.OriginalFile,
                        IsCompiled = x.IsCompiled,
                        Errors = string.Join(",", x.ErrorInfo)
                    };
                }),
                Total = scripts.Count
            };
            return Json(gridModel);
        }

        public IActionResult QueryEditor()
        {
            var model = new QueryEditor();
            return View(model);
        }

        [HttpPost]
        public IActionResult QueryEditor(string query)
        {
            //https://docs.mongodb.com/manual/reference/command/
            if (string.IsNullOrEmpty(query))
                return ErrorForKendoGridJson("Empty query");
            try
            {
                var result = _mongoDBContext.RunCommand<BsonDocument>(query);
                var ok = result.Where(x => x.Name == "ok").FirstOrDefault().Value.ToBoolean();
                var gridModel = new DataSourceResult();
                if (result.Where(x => x.Name == "cursor").ToList().Any())
                {
                    var resultCollection = result["cursor"]["firstBatch"].AsBsonArray.ToList();
                    var response = Serialize(resultCollection);
                    gridModel = new DataSourceResult {
                        Data = response,
                        Total = response.Count()
                    };
                }
                else if (result.Where(x => x.Name == "n").ToList().Any())
                {
                    List<dynamic> n = new List<dynamic>();
                    var number = result["n"].ToInt64();
                    n.Add(new { Number = number });
                    gridModel = new DataSourceResult {
                        Data = n
                    };
                }
                return Json(gridModel);
            }
            catch (Exception ex)
            {
                return ErrorForKendoGridJson(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult RunScript(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new { Result = false, Message = "Empty query!" });

            try
            {
                var bscript = new BsonJavaScript(query);
                var operation = new EvalOperation(_mongoDBContext.Database().DatabaseNamespace, bscript, null);
                var writeBinding = new WritableServerBinding(_mongoDBContext.Database().Client.Cluster, NoCoreSession.NewHandle());
                var result = operation.Execute(writeBinding, CancellationToken.None);
                var xx = result["_ns"];
                return Json(new { Result = true, Message = result.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }
        public IActionResult SeNames()
        {
            var model = new UrlRecordListModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SeNames(DataSourceRequest command, UrlRecordListModel model)
        {
            var urlRecords = await _urlRecordService.GetAllUrlRecords(model.SeName, command.Page - 1, command.PageSize);
            var items = new List<UrlRecordModel>();
            foreach (var x in urlRecords)
            {
                //language
                string languageName;
                if (String.IsNullOrEmpty(x.LanguageId))
                {
                    languageName = _localizationService.GetResource("Admin.System.SeNames.Language.Standard");
                }
                else
                {
                    var language = await _languageService.GetLanguageById(x.LanguageId);
                    languageName = language != null ? language.Name : "Unknown";
                }

                //details URL
                string detailsUrl = "";
                var entityName = x.EntityName != null ? x.EntityName.ToLowerInvariant() : "";
                switch (entityName)
                {
                    case "blogpost":
                        detailsUrl = Url.Action("Edit", "Blog", new { id = x.EntityId });
                        break;
                    case "category":
                        detailsUrl = Url.Action("Edit", "Category", new { id = x.EntityId });
                        break;
                    case "manufacturer":
                        detailsUrl = Url.Action("Edit", "Manufacturer", new { id = x.EntityId });
                        break;
                    case "product":
                        detailsUrl = Url.Action("Edit", "Product", new { id = x.EntityId });
                        break;
                    case "newsitem":
                        detailsUrl = Url.Action("Edit", "News", new { id = x.EntityId });
                        break;
                    case "topic":
                        detailsUrl = Url.Action("Edit", "Topic", new { id = x.EntityId });
                        break;
                    case "vendor":
                        detailsUrl = Url.Action("Edit", "Vendor", new { id = x.EntityId });
                        break;
                    case "course":
                        detailsUrl = Url.Action("Edit", "Course", new { id = x.EntityId });
                        break;
                    case "knowledgebasecategory":
                        detailsUrl = Url.Action("EditCategory", "Knowledgebase", new { id = x.EntityId });
                        break;
                    case "knowledgebasearticle":
                        detailsUrl = Url.Action("EditArticle", "Knowledgebase", new { id = x.EntityId });
                        break;
                    default:
                        break;
                }

                items.Add(new UrlRecordModel {
                    Id = x.Id,
                    Name = x.Slug,
                    EntityId = x.EntityId,
                    EntityName = x.EntityName,
                    IsActive = x.IsActive,
                    Language = languageName,
                    DetailsUrl = detailsUrl
                });

            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = urlRecords.TotalCount
            };
            return Json(gridModel);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSelectedSeNames(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                var urlRecords = new List<UrlRecord>();
                foreach (var id in selectedIds)
                {
                    var urlRecord = await _urlRecordService.GetUrlRecordById(id);
                    if (urlRecord != null)
                        urlRecords.Add(urlRecord);
                }
                foreach (var urlRecord in urlRecords)
                    await _urlRecordService.DeleteUrlRecord(urlRecord);
            }

            return Json(new { Result = true });
        }


        #endregion

        #region Custom css/js/robots.txt

        public async Task<IActionResult> CustomCss()
        {
            var model = new Editor();
            var file = Path.Combine(CommonHelper.WebRootPath, "content", "custom", "style.css");
            if (System.IO.File.Exists(file))
            {
                model.Content = await System.IO.File.ReadAllTextAsync(file);
            }

            if (string.IsNullOrEmpty(model.Content))
                model.Content = "/* my custom style */";

            return View(model);
        }

        public async Task<IActionResult> CustomJs()
        {
            var model = new Editor();
            var file = Path.Combine(CommonHelper.WebRootPath, "content", "custom", "script.js");
            if (System.IO.File.Exists(file))
            {
                model.Content = await System.IO.File.ReadAllTextAsync(file);
            }

            return View(model);
        }

        public async Task<IActionResult> CustomRobotsTxt()
        {
            var model = new Editor();
            var file = Path.Combine(CommonHelper.WebRootPath, "robots.custom.txt");
            if (System.IO.File.Exists(file))
            {
                model.Content = await System.IO.File.ReadAllTextAsync(file);
            }

            return View(model);
        }


        [HttpPost]
        public IActionResult SaveEditor(string content = "", bool css = true)
        {
            try
            {
                var file = Path.Combine(CommonHelper.WebRootPath, "content", "custom", css ? "style.css" : "script.js");

                if (System.IO.File.Exists(file))
                    System.IO.File.WriteAllText(file, content, Encoding.UTF8);
                else
                {
                    if (!Directory.Exists(Path.GetDirectoryName(file)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                    }
                    System.IO.File.WriteAllText(file, content, Encoding.UTF8);
                }
                return Json(_localizationService.GetResource("Admin.Common.Content.Saved"));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult SaveRobotsTxt(string content = "")
        {
            try
            {
                var file = Path.Combine(CommonHelper.WebRootPath, "robots.custom.txt");

                System.IO.File.WriteAllText(file, content, Encoding.UTF8);

                return Json(_localizationService.GetResource("Admin.Common.Content.Saved"));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }



        #endregion
    }
}