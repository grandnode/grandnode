using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Customers;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Security;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Controllers
{
    public partial class CustomerTagController : BaseAdminController
	{
		#region Fields

		private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerTagService _customerTagService;
        #endregion

        #region Constructors

        public CustomerTagController(ICustomerService customerService,
            ILocalizationService localizationService, 
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            ICustomerTagService customerTagService)
		{
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._customerTagService = customerTagService;
        }

        #endregion


        [NonAction]
        protected virtual CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
            };
        }

        #region Customer Tags

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
			return View();
		}

		[HttpPost]
		public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customertags = _customerTagService.GetAllCustomerTags();
            var gridModel = new DataSourceResult
			{
                Data = customertags.Select(x=> new { Id = x.Id, Name = x.Name, Count = _customerTagService.GetCustomerCount(x.Id)}),
                Total = customertags.Count()
			};
            return Json(gridModel);
        }

        [HttpGet]
        public ActionResult Search(string term)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customertags = _customerTagService.GetCustomerTagsByName(term).Select(x=>x.Name);
            return Json
                (
                    customertags, 
                    JsonRequestBehavior.AllowGet
                );
        }



        [HttpPost]
        public ActionResult Customers(string customerTagId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerTagService.GetCustomersByTag(customerTagId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };
            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var model = new CustomerTagModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CustomerTagModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            if (ModelState.IsValid)
            {
                var customertag = model.ToEntity();
                customertag.Name = customertag.Name.ToLower();
                _customerTagService.InsertCustomerTag(customertag);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomerTag", customertag.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerTag"), customertag.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

		public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = customerTag.ToModel();
            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CustomerTagModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var customertag = _customerTagService.GetCustomerTagById(model.Id);
            if (customertag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {

                    customertag = model.ToEntity(customertag);
                    customertag.Name = customertag.Name.ToLower();

                    _customerTagService.UpdateCustomerTag(customertag);

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomerTage", customertag.Id, _localizationService.GetResource("ActivityLog.EditCustomerTag"), customertag.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id}) : RedirectToAction("List");
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customertag.Id });
            }
        }

        [HttpPost]
        public ActionResult CustomerDelete(string Id, string customerTagId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customertag = _customerTagService.GetCustomerTagById(customerTagId);
            if (customertag == null)
                throw new ArgumentException("No customertag found with the specified id");

            _customerTagService.DeleteTagFromCustomer(customerTagId, Id);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                //activity log
                _customerActivityService.InsertActivity("DeleteCustomerTag", customerTag.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerTag"), customerTag.Name);

                _customerTagService.DeleteCustomerTag(customerTag);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerTag.Id });
            }

		}


        #endregion
    }
}
