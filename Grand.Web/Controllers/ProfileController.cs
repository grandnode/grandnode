using System;
using Microsoft.AspNetCore.Mvc;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Media;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Models.Profile;
using Grand.Services.Security;

namespace Grand.Web.Controllers
{
    public partial class ProfileController : BasePublicController
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public ProfileController(IForumService forumService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
            ForumSettings forumSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings)
        {
            this._forumService = forumService;
            this._localizationService = localizationService;
            this._pictureService = pictureService;
            this._countryService = countryService;
            this._customerService = customerService;
            this._permissionService = permissionService;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
            this._mediaSettings = mediaSettings;
        }

        public virtual IActionResult Index(string id, int? pageNumber)
        {
            if (!_customerSettings.AllowViewingProfiles)
            {
                return RedirectToRoute("HomePage");
            }

            var customerId ="";
            if (!String.IsNullOrEmpty(id))
            {
                customerId = id;
            }

            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null || customer.IsGuest())
            {
                return RedirectToRoute("HomePage");
            }

            bool pagingPosts = false;
            int postsPage = 0;

            if (pageNumber.HasValue)
            {
                postsPage = pageNumber.Value;
                pagingPosts = true;
            }

            var name = customer.FormatUserName();
            var title = string.Format(_localizationService.GetResource("Profile.ProfileOf"), name);

            var model = new ProfileIndexModel
            {
                ProfileTitle = title,
                PostsPage = postsPage,
                PagingPosts = pagingPosts,
                CustomerProfileId = customer.Id,
                ForumsEnabled = _forumSettings.ForumsEnabled
            };

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                DisplayEditLink(Url.Action("Edit", "Customer", new { id = customer.Id, area = "Admin" }));

            return View(model);
        }
    }
}
