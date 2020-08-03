using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class ProfileController : BasePublicController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;

        public ProfileController(
            ILocalizationService localizationService,
            ICustomerService customerService,
            IPermissionService permissionService,
            ForumSettings forumSettings,
            CustomerSettings customerSettings)
        {
            _localizationService = localizationService;
            _customerService = customerService;
            _permissionService = permissionService;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
        }

        public virtual async Task<IActionResult> Index(string id, int? pageNumber)
        {
            if (!_customerSettings.AllowViewingProfiles)
            {
                return RedirectToRoute("HomePage");
            }

            var customerId = "";
            if (!String.IsNullOrEmpty(id))
            {
                customerId = id;
            }

            var customer = await _customerService.GetCustomerById(customerId);
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

            var name = customer.FormatUserName(_customerSettings.CustomerNameFormat);
            var title = string.Format(_localizationService.GetResource("Profile.ProfileOf"), name);

            var model = new ProfileIndexModel {
                ProfileTitle = title,
                PostsPage = postsPage,
                PagingPosts = pagingPosts,
                CustomerProfileId = customer.Id,
                ForumsEnabled = _forumSettings.ForumsEnabled
            };

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                DisplayEditLink(Url.Action("Edit", "Customer", new { id = customer.Id, area = "Admin" }));

            return View(model);
        }
    }
}
