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
using Grand.Services.Common;
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
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Grand.Web.Areas.Admin.Controllers
{
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
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ISearchTermService _searchTermService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IRepository<Product> _repositoryProduct;
        private readonly CatalogSettings _catalogSettings;
        private readonly IHttpContextAccessor _httpContext;
        private readonly GrandConfig _grandConfig;
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
            IPermissionService permissionService, 
            ILocalizationService localizationService,
            ISearchTermService searchTermService,
            ISettingService settingService,
            IStoreService storeService,
            IRepository<Product> repositoryProduct,
            CatalogSettings catalogSettings,
            IHttpContextAccessor httpContext,
            GrandConfig grandConfig
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
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._searchTermService = searchTermService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._repositoryProduct = repositoryProduct;
            this._catalogSettings = catalogSettings;
            this._httpContext = httpContext;
            this._grandConfig = grandConfig;
        }

        #endregion

        #region Methods

        public IActionResult SystemInfo()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

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

            var machineNameProvider = EngineContext.Current.Resolve<IMachineNameProvider>();
            model.MachineName = machineNameProvider.GetMachineName();

            model.ServerTimeZone = TimeZoneInfo.Local.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.UtcTime = DateTime.UtcNow;
            foreach (var header in HttpContext.Request.Headers)
            {
                model.ServerVariables.Add(new SystemInfoModel.ServerVariableModel
                {
                    Name = header.Key,
                    Value = header.Value
                });
            }
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                model.LoadedAssemblies.Add(new SystemInfoModel.LoadedAssembly
                {
                    FullName = assembly.FullName,                    
                });
            }
            return View(model);
        }


        public IActionResult Warnings()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new List<SystemWarningModel>();

            //store URL
            var currentStoreUrl = _storeContext.CurrentStore.Url;
            if (!String.IsNullOrEmpty(currentStoreUrl) &&
                (currentStoreUrl.Equals(_webHelper.GetStoreLocation(false), StringComparison.OrdinalIgnoreCase)
                ||
                currentStoreUrl.Equals(_webHelper.GetStoreLocation(true), StringComparison.OrdinalIgnoreCase)
                ))
                model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Pass,
                        Text = _localizationService.GetResource("Admin.System.Warnings.URL.Match")
                    });
            else
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.URL.NoMatch"), currentStoreUrl, _webHelper.GetStoreLocation(false))
                });


            //primary exchange rate currency
            var perCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
            if (perCurrency != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Set"),
                });
                if (perCurrency.Rate != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.Rate1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.ExchangeCurrency.NotSet")
                });
            }

            //primary store currency
            var pscCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (pscCurrency != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.Set"),
                });
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PrimaryCurrency.NotSet")
                });
            }


            //base measure weight
            var bWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (bWeight != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Set"),
                });

                if (bWeight.Ratio != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultWeight.NotSet")
                });
            }


            //base dimension weight
            var bDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (bDimension != null)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Set"),
                });

                if (bDimension.Ratio != 1)
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Fail,
                        Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.Ratio1")
                    });
                }
            }
            else
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DefaultDimension.NotSet")
                });
            }

            //shipping rate coputation methods
            var srcMethods = _shippingService.LoadActiveShippingRateComputationMethods();
            if (srcMethods.Count == 0)
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.NoComputationMethods")
                });
            if (srcMethods.Count(x => x.ShippingRateComputationMethodType == ShippingRateComputationMethodType.Offline) > 1)
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Shipping.OnlyOneOffline")
                });

            //payment methods
            if (_paymentService.LoadActivePaymentMethods().Any())
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.OK")
                });
            else
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = _localizationService.GetResource("Admin.System.Warnings.PaymentMethods.NoActive")
                });

            //incompatible plugins
            if (PluginManager.IncompatiblePlugins != null)
                foreach (var pluginName in PluginManager.IncompatiblePlugins)
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.IncompatiblePlugin"), pluginName )
                    });

            //performance settings
            if (!_catalogSettings.IgnoreStoreLimitations && _storeService.GetAllStores().Count == 1)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = _localizationService.GetResource("Admin.System.Warnings.Performance.IgnoreStoreLimitations")
                });
            }
            if (!_catalogSettings.IgnoreAcl)
            {
                model.Add(new SystemWarningModel
                {
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
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.Wrong"), WindowsIdentity.GetCurrent().Name, dir)
                    });
                    dirPermissionsOk = false;
                }
            if (dirPermissionsOk)
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.DirectoryPermission.OK")
                });

            var filePermissionsOk = true;
            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (string file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                {
                    model.Add(new SystemWarningModel
                    {
                        Level = SystemWarningLevel.Warning,
                        Text = string.Format(_localizationService.GetResource("Admin.System.Warnings.FilePermission.Wrong"), WindowsIdentity.GetCurrent().Name, file)
                    });
                    filePermissionsOk = false;
                }
            if (filePermissionsOk)
            {
                model.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = _localizationService.GetResource("Admin.System.Warnings.FilePermission.OK")
                });
            }
            return View(model);
        }


        public IActionResult Maintenance()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new MaintenanceModel();
            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;
            model.DeleteAbandonedCarts.OlderThan = DateTime.UtcNow.AddDays(-182);
            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-guests")]
        public IActionResult MaintenanceDeleteGuests(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            DateTime? startDateValue = (model.DeleteGuests.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteGuests.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteGuests.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            model.DeleteGuests.NumberOfDeletedCustomers = _customerService.DeleteGuestCustomers(startDateValue, endDateValue, model.DeleteGuests.OnlyWithoutShoppingCart);

            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("clear-most-view")]
        public IActionResult MaintenanceClearMostViewed(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var update = new UpdateDefinitionBuilder<Product>().Set(x => x.Viewed, 0);
            var result = _repositoryProduct.Collection.UpdateManyAsync(x => x.Viewed != 0, update).Result;

            return View(model);
        }
        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-exported-files")]
        public IActionResult MaintenanceDeleteFiles(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            DateTime? startDateValue = (model.DeleteExportedFiles.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.DeleteExportedFiles.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DeleteExportedFiles.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
 
            model.DeleteExportedFiles.NumberOfDeletedFiles = 0;
            return View(model);
        }


        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("delete-activitylog")]
        public IActionResult MaintenanceDeleteActivitylog(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var _activityLogRepository = EngineContext.Current.Resolve<IRepository<ActivityLog>>();
            _activityLogRepository.Collection.DeleteMany(new MongoDB.Bson.BsonDocument());
            model.DeleteActivityLog = true;            
            return View(model);
        }

        public IActionResult SetLanguage(string langid, string returnUrl = "")
        {
            var language = _languageService.GetLanguageById(langid);
            if (language != null)
            {
                _workContext.WorkingLanguage = language;
            }

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = "Admin" });
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            return Redirect(returnUrl);
        }


        public IActionResult ClearCache(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var cacheManager = EngineContext.Current.Resolve<ICacheManager>();
            cacheManager.Clear();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();
            
            return View(_grandConfig.UseRoslynScripts);
        }

        [HttpPost]
        public IActionResult Roslyn(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var scripts = RoslynCompiler.ReferencedScripts != null ? RoslynCompiler.ReferencedScripts.ToList() : new List<ResultCompiler>();

            var gridModel = new DataSourceResult
            {
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

        public IActionResult SeNames()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new UrlRecordListModel();
            return View(model);
        }
        [HttpPost]
        public IActionResult SeNames(DataSourceRequest command, UrlRecordListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var urlRecords = _urlRecordService.GetAllUrlRecords(model.SeName, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = urlRecords.Select(x =>
                {
                    //language
                    string languageName;
                    if (String.IsNullOrEmpty(x.LanguageId))
                    {
                        languageName = _localizationService.GetResource("Admin.System.SeNames.Language.Standard");
                    }
                    else
                    {
                        var language = _languageService.GetLanguageById(x.LanguageId);
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
                        case "knowledgebasecategory":
                            detailsUrl = Url.Action("EditCategory", "Knowledgebase", new { id = x.EntityId });
                            break;
                        case "knowledgebasearticle":
                            detailsUrl = Url.Action("EditArticle", "Knowledgebase", new { id = x.EntityId });
                            break;
                        default:
                            break;
                    }

                    return new UrlRecordModel
                    {
                        Id = x.Id,
                        Name = x.Slug,
                        EntityId = x.EntityId,
                        EntityName = x.EntityName,
                        IsActive = x.IsActive,
                        Language = languageName,
                        DetailsUrl = detailsUrl
                    };
                }),
                Total = urlRecords.TotalCount
            };
            return Json(gridModel);
        }
        [HttpPost]
        public IActionResult DeleteSelectedSeNames(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var urlRecords = new List<UrlRecord>();
                foreach (var id in selectedIds)
                {
                    var urlRecord = _urlRecordService.GetUrlRecordById(id);
                    if (urlRecord != null)
                        urlRecords.Add(urlRecord);
                }
                foreach (var urlRecord in urlRecords)
                    _urlRecordService.DeleteUrlRecord(urlRecord);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult PopularSearchTermsReport(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var searchTermRecordLines = _searchTermService.GetStats(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = searchTermRecordLines.Select(x => new SearchTermReportLineModel
                {
                    Keyword = x.Keyword,
                    Count = x.Count,
                }),
                Total = searchTermRecordLines.TotalCount
            };
            return Json(gridModel);
        }

        #endregion
    }
}