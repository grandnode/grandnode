using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class VendorReviewController : BaseAdminController
    {
        #region Fields

        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Constructors

        public VendorReviewController(IVendorService vendorService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            this._vendorService = vendorService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareVendorReviewModel(VendorReviewModel model,
            VendorReview vendorReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");
            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(vendorReview.CustomerId);
            model.Id = vendorReview.Id;
            model.VendorId = vendorReview.VendorId;
            model.VendorName = vendor.Name;
            model.CustomerId = vendorReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = vendorReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorReview.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.Title = vendorReview.Title;
                if (formatReviewText)
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(vendorReview.ReviewText, false, true, false, false, false, false);
                else
                    model.ReviewText = vendorReview.ReviewText;
                model.IsApproved = vendorReview.IsApproved;
            }
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            var model = new VendorReviewListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, VendorReviewListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            IPagedList<VendorReview> vendorReviews;
            //vendor
            if (_workContext.CurrentVendor != null)
            {
                vendorReviews = _vendorService.GetAllVendorReviews("", null,
                    createdOnFromValue, createdToFromValue, model.SearchText, _workContext.CurrentVendor.Id, command.Page - 1, command.PageSize);
            }
            //admin
            else if (_workContext.CurrentCustomer.IsAdmin())
            {
                vendorReviews = _vendorService.GetAllVendorReviews("", null,
                    createdOnFromValue, createdToFromValue, model.SearchText, model.SearchVendorId, command.Page - 1, command.PageSize);
            }
            else
                return AccessDeniedView();

            var gridModel = new DataSourceResult
            {
                Data = vendorReviews.Select(x =>
                {
                    var m = new VendorReviewModel();
                    PrepareVendorReviewModel(m, x, false, true);
                    return m;
                }),
                Total = vendorReviews.TotalCount,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            var vendorReview = _vendorService.GetVendorReviewById(id);
            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);

            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            var model = new VendorReviewModel();
            PrepareVendorReviewModel(model, vendorReview, false, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(VendorReviewModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews) || _workContext.CurrentVendor != null)
                return AccessDeniedView();

            var vendorReview = _vendorService.GetVendorReviewById(model.Id);
            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);


            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                vendorReview.Title = model.Title;
                vendorReview.ReviewText = model.ReviewText;
                vendorReview.IsApproved = model.IsApproved;

                _vendorService.UpdateVendorReview(vendorReview);

                //update vendor totals
                _vendorService.UpdateVendorReviewTotals(vendor);

                SuccessNotification(_localizationService.GetResource("Admin.VendorReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendorReview.Id, VendorId = vendorReview.VendorId }) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            PrepareVendorReviewModel(model, vendorReview, true, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            var vendorReview = _vendorService.GetVendorReviewById(id);
            if (vendorReview == null)
                //No vendor review found with the specified id
                return RedirectToAction("List");

            _vendorService.DeleteVendorReview(vendorReview);

            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            //update vendor totals
            _vendorService.UpdateVendorReviewTotals(vendor);

            SuccessNotification(_localizationService.GetResource("Admin.VendorReviews.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult ApproveSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    string idReview = id.Split(':').First().ToString();
                    string idVendor = id.Split(':').Last().ToString();
                    var vendor = _vendorService.GetVendorById(idVendor);
                    var vendorReview = _vendorService.GetVendorReviewById(idReview);
                    if (vendorReview != null)
                    {
                        var previousIsApproved = vendorReview.IsApproved;
                        vendorReview.IsApproved = true;
                        _vendorService.UpdateVendorReview(vendorReview);
                        _vendorService.UpdateVendorReviewTotals(vendor);

                        //raise event (only if it wasn't approved before)
                        if (!previousIsApproved)
                            _eventPublisher.Publish(new VendorReviewApprovedEvent(vendorReview));
                    }
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult DisapproveSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendorReviews))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    string idReview = id.Split(':').First().ToString();
                    string idVendor = id.Split(':').Last().ToString();

                    var vendor = _vendorService.GetVendorById(idVendor);
                    var vendorReview = _vendorService.GetVendorReviewById(idReview);
                    if (vendorReview != null)
                    {
                        vendorReview.IsApproved = false;
                        _vendorService.UpdateVendorReview(vendorReview);
                        _vendorService.UpdateVendorReviewTotals(vendor);
                    }
                }
            }

            return Json(new { Result = true });
        }


        public IActionResult VendorSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            var vendors = _vendorService.SearchVendors(
                keywords: term);

            var result = (from p in vendors
                          select new
                          {
                              label = p.Name,
                              vendorid = p.Id
                          })
                .ToList();
            return Json(result);
        }
        #endregion
    }
}