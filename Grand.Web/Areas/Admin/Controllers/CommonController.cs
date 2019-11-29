using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Seo;
using Grand.Core.Infrastructure;
using Grand.Core.Plugins;
using Grand.Core.Roslyn;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security;
using Grand.Framework.Security.Authorization;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Infrastructure;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
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
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IRepository<Product> _repositoryProduct;
        private readonly CatalogSettings _catalogSettings;
        private readonly IHttpContextAccessor _httpContext;
        private readonly GrandConfig _grandConfig;
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Constructors

        public CommonController(IPaymentService paymentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ICurrencyService currencyService,
            IMeasureService measureService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            CurrencySettings currencySettings,
            MeasureSettings measureSettings,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreService storeService,
            IRepository<Product> repositoryProduct,
            CatalogSettings catalogSettings,
            IHttpContextAccessor httpContext,
            GrandConfig grandConfig,
            IMongoDBContext mongoDBContext,
            IServiceProvider serviceProvider
            )
        {
            this._paymentService = paymentService;
            this._shippingService = shippingService;
            this._shoppingCartService = shoppingCartService;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._webHelper = webHelper;
            this._currencySettings = currencySettings;
            this._measureSettings = measureSettings;
            this._dateTimeHelper = dateTimeHelper;
            this._languageService = languageService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._repositoryProduct = repositoryProduct;
            this._catalogSettings = catalogSettings;
            this._httpContext = httpContext;
            this._grandConfig = grandConfig;
            this._mongoDBContext = mongoDBContext;
            this._serviceProvider = serviceProvider;
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
            model.GrandVersion = GrandVersion.CurrentVersion;
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

            var machineNameProvider = _serviceProvider.GetRequiredService<IMachineNameProvider>();
            model.MachineName = machineNameProvider.GetMachineName();

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
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
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
            var update = new UpdateDefinitionBuilder<Product>().Set(x => x.Viewed, 0);
            await _repositoryProduct.Collection.UpdateManyAsync(x => x.Viewed != 0, update);
            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-exported-files")]
        public IActionResult MaintenanceDeleteFiles(MaintenanceModel model)
        {
            DateTime? startDateValue = (model.DeleteExportedFiles.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteExportedFiles.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //TO DO
            model.DeleteExportedFiles.NumberOfDeletedFiles = 0;
            return View(model);
        }


        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-activitylog")]
        public async Task<IActionResult> MaintenanceDeleteActivitylog(MaintenanceModel model)
        {
            var _activityLogRepository = _serviceProvider.GetRequiredService<IRepository<ActivityLog>>();
            await _activityLogRepository.Collection.DeleteManyAsync(new MongoDB.Bson.BsonDocument());
            model.DeleteActivityLog = true;
            return View(model);
        }

        public async Task<IActionResult> ClearCache(bool memory, string returnUrl = "")
        {
            var cacheManagers = _serviceProvider.GetRequiredService<IEnumerable<ICacheManager>>();
            foreach (var cacheManager in cacheManagers)
            {
                if (memory)
                {
                    if (cacheManager is MemoryCacheManager)
                        await cacheManager.Clear();
                }
                else
                {
                    if (!(cacheManager is MemoryCacheManager))
                        await cacheManager.Clear();
                }
            }
            //home page
            if (String.IsNullOrEmpty(returnUrl))
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
    }
}