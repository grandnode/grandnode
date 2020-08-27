using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Affiliates;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Affiliates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Affiliates)]
    public partial class AffiliateController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IAffiliateService _affiliateService;
        private readonly IAffiliateViewModelService _affiliateViewModelService;

        #endregion

        #region Constructors

        public AffiliateController(ILocalizationService localizationService,
            IAffiliateService affiliateService, IAffiliateViewModelService affiliateViewModelService)
        {
            _localizationService = localizationService;
            _affiliateService = affiliateService;
            _affiliateViewModelService = affiliateViewModelService;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new AffiliateListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, AffiliateListModel model)
        {
            var affiliatesModel = await _affiliateViewModelService.PrepareAffiliateModelList(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = affiliatesModel.affiliateModels,
                Total = affiliatesModel.totalCount,
            };
            return Json(gridModel);
        }

        //create
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new AffiliateModel();
            await _affiliateViewModelService.PrepareAffiliateModel(model, null, false);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Create(AffiliateModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var affiliate = await _affiliateViewModelService.InsertAffiliateModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = affiliate.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _affiliateViewModelService.PrepareAffiliateModel(model, null, true);
            return View(model);

        }


        //edit
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var affiliate = await _affiliateService.GetAffiliateById(id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            var model = new AffiliateModel();
            await _affiliateViewModelService.PrepareAffiliateModel(model, affiliate, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(AffiliateModel model, bool continueEditing)
        {
            var affiliate = await _affiliateService.GetAffiliateById(model.Id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                affiliate = await _affiliateViewModelService.UpdateAffiliateModel(model, affiliate);

                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = affiliate.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _affiliateViewModelService.PrepareAffiliateModel(model, affiliate, true);
            return View(model);
        }

        //delete
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var affiliate = await _affiliateService.GetAffiliateById(id);
            if (affiliate == null)
                //No affiliate found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                await _affiliateService.DeleteAffiliate(affiliate);
                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> AffiliatedOrderList(DataSourceRequest command, AffiliatedOrderListModel model)
        {
            var affiliate = await _affiliateService.GetAffiliateById(model.AffliateId);
            if (affiliate == null)
                throw new ArgumentException("No affiliate found with the specified id");

            var affiliateOrders = await _affiliateViewModelService.PrepareAffiliatedOrderList(affiliate, model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = affiliateOrders.affiliateOrderModels,
                Total = affiliateOrders.totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> AffiliatedCustomerList(string affiliateId, DataSourceRequest command)
        {
            var affiliate = await _affiliateService.GetAffiliateById(affiliateId);
            if (affiliate == null)
                throw new ArgumentException("No affiliate found with the specified id");

            var affiliateCustomers = await _affiliateViewModelService.PrepareAffiliatedCustomerList(affiliate, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = affiliateCustomers.affiliateCustomerModels,
                Total = affiliateCustomers.totalCount
            };

            return Json(gridModel);
        }
        #endregion
    }
}
