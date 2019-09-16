using Grand.Core;
using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CourseController : BasePublicController
    {
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IWorkContext _workContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly ICourseViewModelService _courseViewModelService;
        private readonly CourseSettings _courseSettings;

        public CourseController(IPermissionService permissionService, IAclService aclService,
            IWorkContext workContext, IStoreMappingService storeMappingService, ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService, IWebHelper webHelper, IStoreContext storeContext,
            ILocalizationService localizationService, ICustomerActionEventService customerActionEventService, ICourseViewModelService courseViewModelService,
            CourseSettings courseSettings)
        {
            _permissionService = permissionService;
            _aclService = aclService;
            _workContext = workContext;
            _storeMappingService = storeMappingService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _customerActionEventService = customerActionEventService;
            _courseViewModelService = courseViewModelService;
            _courseSettings = courseSettings;
        }

        public virtual async Task<IActionResult> Details(string courseId)
        {
            var customer = _workContext.CurrentCustomer;

            //Check whether the current user is a guest
            if (customer.IsGuest() && !_courseSettings.AllowGuestsToAccessCourse)
            {
                return InvokeHttp404();
            }

            var course = await _courseViewModelService.GetCourseById(courseId);
            if (course == null)
                return InvokeHttp404();

            //Check whether the current user has a "Manage course" permission
            //It allows him to preview a category before publishing
            if (!course.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                return InvokeHttp404();

            //Check whether the current user purchased the course
            if (!await _courseViewModelService.CheckOrder(course, customer) && !await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(course, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(course))
                return InvokeHttp404();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false), _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                DisplayEditLink(Url.Action("Edit", "Course", new { id = course.Id, area = "Admin" }));

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewCourse", course.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewCourse"), course.Name);
            await _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");

            //model
            var model = await _courseViewModelService.PrepareCourseModel(course);

            return View(model);
        }
    }
}
