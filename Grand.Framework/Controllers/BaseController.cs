﻿using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure;
using Grand.Framework.Events;
using Grand.Framework.Kendoui;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.UI;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Stores;
using MediatR;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Grand.Framework.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    [SignOutFromExternalAuthentication]
    [ValidatePassword]
    [SaveIpAddress]
    [SaveLastActivity]
    [SaveLastVisitedPage]
    public abstract class BaseController : Controller
    {

        #region Rendering

        /// <summary>
        /// Render componentto string
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <returns>Result</returns>
        protected virtual string RenderViewComponentToString(string componentName, object arguments = null)
        {
            //original implementation: https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.ViewFeatures/Internal/ViewComponentResultExecutor.cs
            //we customized it to allow running from controllers

            //TODO add support for parameters (pass ViewComponent as input parameter)
            if (String.IsNullOrEmpty(componentName))
                throw new ArgumentNullException("componentName");

            var actionContextAccessor = HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            if (actionContextAccessor == null)
                throw new Exception("IActionContextAccessor cannot be resolved");

            var context = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

            var viewComponentResult = ViewComponent(componentName, arguments);

            var viewData = ViewData;
            if (viewData == null)
            {
                throw new NotImplementedException();
                //TODO viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            }

            var tempData = TempData;
            if (tempData == null)
            {
                throw new NotImplementedException();
                //TODO tempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext);
            }

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    context,
                    NullView.Instance,
                    viewData,
                    tempData,
                    writer,
                    new HtmlHelperOptions());

                // IViewComponentHelper is stateful, we want to make sure to retrieve it every time we need it.
                var viewComponentHelper = context.HttpContext.RequestServices.GetRequiredService<IViewComponentHelper>();
                (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);

                Task<IHtmlContent> result = viewComponentResult.ViewComponentType == null ?
                    viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentName, viewComponentResult.Arguments) :
                    viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentType, viewComponentResult.Arguments);

                result.Result.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <returns>Result</returns>
        protected virtual async Task<string> RenderPartialViewToString()
        {
            return await RenderPartialViewToString(null, null);
        }

        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>Result</returns>
        protected virtual async Task<string> RenderPartialViewToString(string viewName)
        {
            return await RenderPartialViewToString(viewName, null);
        }

        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected virtual async Task<string> RenderPartialViewToString(object model)
        {
            return await RenderPartialViewToString(null, model);
        }

        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected virtual async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            //get Razor view engine
            var razorViewEngine = HttpContext.RequestServices.GetRequiredService<IRazorViewEngine>();

            //create action context
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

            //set view name as action name in case if not passed
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            //set model
            ViewData.Model = model;
            var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
            if (viewResult.View == null)
            {
                //or try to get a view by the path
                viewResult = razorViewEngine.GetView(null, viewName, false);
                if (viewResult.View == null)
                    throw new ArgumentNullException($"{viewName} view was not found");
            }


            using (var stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(actionContext, viewResult.View, ViewData, TempData, stringWriter, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Display success notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }

        /// <summary>
        /// Display warning notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void WarningNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Warning, message, persistForTheNextRequest);
        }

        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void ErrorNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Error, message, persistForTheNextRequest);
        }

        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void ErrorNotification(ModelStateDictionary ModelState, bool persistForTheNextRequest = true)
        {
            var modelErrors = new List<string>();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var modelError in modelState.Errors)
                {
                    modelErrors.Add(modelError.ErrorMessage);
                }
            }
            AddNotification(NotifyType.Error, string.Join(',', modelErrors), persistForTheNextRequest);
        }

        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true, bool logException = true)
        {
            if (logException)
                LogException(exception);

            AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exception">Exception</param>
        protected void LogException(Exception exception)
        {
            var workContext = HttpContext.RequestServices.GetRequiredService<IWorkContext>();
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger>();
            var customer = workContext.CurrentCustomer;
            logger.Error(exception.Message, exception, customer);
        }

        /// <summary>
        /// Display notification
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            var dataKey = string.Format("grand.notifications.{0}", type);

            if (persistForTheNextRequest)
            {
                //1. Compare with null (first usage)
                //2. For some unknown reasons sometimes List<string> is converted to string[]. And it throws exceptions. That's why we reset it
                if (TempData[dataKey] == null || !(TempData[dataKey] is List<string>))
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                //1. Compare with null (first usage)
                //2. For some unknown reasons sometimes List<string> is converted to string[]. And it throws exceptions. That's why we reset it
                if (ViewData[dataKey] == null || !(ViewData[dataKey] is List<string>))
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }

        /// <summary>
        /// Error's json data for kendo grid
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <returns>Error's json data</returns>
        protected JsonResult ErrorForKendoGridJson(string errorMessage)
        {
            var gridModel = new DataSourceResult {
                Errors = errorMessage
            };

            return Json(gridModel);
        }
        /// <summary>
        /// Error's json data for kendo grid
        /// </summary>
        /// <param name="modelState">Model state</param>
        /// <returns>Error's json data</returns>
        protected JsonResult ErrorForKendoGridJson(ModelStateDictionary modelState)
        {
            var gridModel = new DataSourceResult {
                Errors = ModelState.SerializeErrors()
            };
            return Json(gridModel);
        }
        /// <summary>
        /// Display "Edit" (manage) link (in public store)
        /// </summary>
        /// <param name="editPageUrl">Edit page URL</param>
        protected virtual void DisplayEditLink(string editPageUrl)
        {
            var pageHeadBuilder = HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();

            pageHeadBuilder.AddEditPageUrl(editPageUrl);
        }


        /// <summary>
        /// Get active store scope (for multi-store configuration mode)
        /// </summary>
        /// <param name="storeService">Store service</param>
        /// <param name="workContext">Work context</param>
        /// <returns>Store ID; 0 if we are in a shared mode</returns>
        protected virtual async Task<string> GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if ((await storeService.GetAllStores()).Count < 2)
                return "";

            var storeId = workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
            var store = await storeService.GetStoreById(storeId);

            return store != null ? store.Id : "";
        }

        #endregion

        #region Localization

        /// <summary>
        /// Add locales for localizable entities
        /// </summary>
        /// <typeparam name="TLocalizedModelLocal">Localizable model</typeparam>
        /// <param name="languageService">Language service</param>
        /// <param name="locales">Locales</param>
        protected virtual async Task AddLocales<TLocalizedModelLocal>(ILanguageService languageService,
            IList<TLocalizedModelLocal> locales) where TLocalizedModelLocal : ILocalizedModelLocal
        {
            await AddLocales(languageService, locales, null);
        }

        /// <summary>
        /// Add locales for localizable entities
        /// </summary>
        /// <typeparam name="TLocalizedModelLocal">Localizable model</typeparam>
        /// <param name="languageService">Language service</param>
        /// <param name="locales">Locales</param>
        /// <param name="configure">Configure action</param>
        protected virtual async Task AddLocales<TLocalizedModelLocal>(ILanguageService languageService,
            IList<TLocalizedModelLocal> locales, Action<TLocalizedModelLocal, string> configure) where TLocalizedModelLocal : ILocalizedModelLocal
        {
            foreach (var language in await languageService.GetAllLanguages(true))
            {
                var locale = Activator.CreateInstance<TLocalizedModelLocal>();
                locale.LanguageId = language.Id;

                if (configure != null)
                    configure.Invoke(locale, locale.LanguageId);

                locales.Add(locale);
            }
        }


        #endregion

        #region Security

        /// <summary>
        /// Access denied view
        /// </summary>
        /// <returns>Access denied view</returns>
        protected virtual IActionResult AccessDeniedView()
        {
            var webHelper = HttpContext.RequestServices.GetRequiredService<IWebHelper>();
            return RedirectToAction("AccessDenied", "Security", new { pageUrl = webHelper.GetRawUrl(this.Request) });
        }

        /// <summary>
        /// Access denied json data for kendo grid
        /// </summary>
        /// <returns>Access denied json data</returns>
        protected JsonResult AccessDeniedKendoGridJson()
        {
            var localizationService = HttpContext.RequestServices.GetRequiredService<ILocalizationService > ();
            return ErrorForKendoGridJson(localizationService.GetResource("Admin.AccessDenied.Description"));
        }

        #endregion

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // event notification before execute
            var mediator = context.HttpContext.RequestServices.GetService<IMediator>();
            await mediator.Publish(new ActionExecutingContextNotification(context, true));

            await next();

            //event notification after execute
            await mediator.Publish(new ActionExecutingContextNotification(context, false));
        }
    }
}