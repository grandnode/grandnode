using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Customers)]
    public partial class CustomerTagController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerTagViewModelService _customerTagViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerTagService _customerTagService;
        #endregion

        #region Constructors

        public CustomerTagController(
            ICustomerTagViewModelService customerTagViewModelService,
            ILocalizationService localizationService,
            ICustomerTagService customerTagService)
        {
            this._customerTagViewModelService = customerTagViewModelService;
            this._localizationService = localizationService;
            this._customerTagService = customerTagService;
        }

        #endregion

        #region Customer Tags

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var customertags = _customerTagService.GetAllCustomerTags();
            var gridModel = new DataSourceResult
            {
                Data = customertags.Select(x => new { Id = x.Id, Name = x.Name, Count = _customerTagService.GetCustomerCount(x.Id) }),
                Total = customertags.Count()
            };
            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            var customertags = _customerTagService.GetCustomerTagsByName(term).Select(x => x.Name);
            return Json(customertags);
        }



        [HttpPost]
        public IActionResult Customers(string customerTagId, DataSourceRequest command)
        {
            var customers = _customerTagService.GetCustomersByTag(customerTagId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(x => _customerTagViewModelService.PrepareCustomerModelForList(x)),
                Total = customers.TotalCount
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = _customerTagViewModelService.PrepareCustomerTagModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerTagModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customertag = _customerTagViewModelService.InsertCustomerTagModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = customerTag.ToModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerTagModel model, bool continueEditing)
        {
            var customertag = _customerTagService.GetCustomerTagById(model.Id);
            if (customertag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    customertag = _customerTagViewModelService.UpdateCustomerTagModel(customertag, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id }) : RedirectToAction("List");
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
        public IActionResult CustomerDelete(string Id, string customerTagId)
        {
            var customertag = _customerTagService.GetCustomerTagById(customerTagId);
            if (customertag == null)
                throw new ArgumentException("No customertag found with the specified id");
            if (ModelState.IsValid)
            {
                _customerTagService.DeleteTagFromCustomer(customerTagId, Id);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    _customerTagViewModelService.DeleteCustomerTag(customerTag);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Deleted"));
                    return RedirectToAction("List");
                }
                ErrorNotification(ModelState);
                return RedirectToAction("Edit", new { id = customerTag.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerTag.Id });
            }
        }
        #endregion

        #region Products

        [HttpPost]
        public IActionResult Products(string customerTagId, DataSourceRequest command, [FromServices] IProductService productService)
        {
            var products = _customerTagService.GetCustomerTagProducts(customerTagId);

            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new CustomerRoleProductModel
                {
                    Id = x.Id,
                    Name = productService.GetProductById(x.ProductId)?.Name,
                    ProductId = x.ProductId,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = products.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductDelete(string id)
        {
            var ctp = _customerTagService.GetCustomerTagProductById(id);
            if (ctp == null)
                throw new ArgumentException("No found the specified id");
            if (ModelState.IsValid)
            {
                _customerTagService.DeleteCustomerTagProduct(ctp);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductUpdate(CustomerRoleProductModel model)
        {
            var ctp = _customerTagService.GetCustomerTagProductById(model.Id);
            if (ctp == null)
                throw new ArgumentException("No customer tag product found with the specified id");
            if (ModelState.IsValid)
            {
                ctp.DisplayOrder = model.DisplayOrder;
                _customerTagService.UpdateCustomerTagProduct(ctp);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ProductAddPopup(string customerTagId)
        {
            var model = _customerTagViewModelService.PrepareProductModel(customerTagId);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerTagProductModel.AddProductModel model)
        {
            var products = _customerTagViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = products.products,
                Total = products.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(CustomerTagProductModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _customerTagViewModelService.InsertProductModel(model);
            }

            //a vendor should have access only to his products
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion
    }
}
