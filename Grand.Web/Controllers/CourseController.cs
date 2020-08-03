using Grand.Core;
using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Courses;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Commands.Models.Courses;
using Grand.Web.Features.Models.Courses;
using MediatR;
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
        private readonly ICourseService _courseService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly IDownloadService _downloadService;
        private readonly IMediator _mediator;
        private readonly CourseSettings _courseSettings;

        public CourseController(
            IPermissionService permissionService, 
            IAclService aclService, 
            IWorkContext workContext, 
            IStoreMappingService storeMappingService, 
            ICustomerActivityService customerActivityService, 
            IGenericAttributeService genericAttributeService, 
            IWebHelper webHelper, 
            IStoreContext storeContext, 
            ILocalizationService localizationService, 
            ICustomerActionEventService customerActionEventService, 
            ICourseService courseService, 
            ICourseLessonService courseLessonService, 
            IDownloadService downloadService, 
            IMediator mediator, 
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
            _courseService = courseService;
            _courseLessonService = courseLessonService;
            _downloadService = downloadService;
            _mediator = mediator;
            _courseSettings = courseSettings;
        }

        protected async Task<bool> CheckPermission(Course course, Customer customer)
        {
            //Check whether the current user is a guest
            if (customer.IsGuest() && !_courseSettings.AllowGuestsToAccessCourse)
            {
                return false;
            }

            //Check whether the current user has a "Manage course" permission
            //It allows him to preview a category before publishing
            if (!course.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                return false;

            //Check whether the current user purchased the course
            if (!await _mediator.Send(new GetCheckOrder() { Course = course, Customer = customer })
                && !await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                return false;

            //ACL (access control list)
            if (!_aclService.Authorize(course, customer))
                return false;

            //Store mapping
            if (!_storeMappingService.Authorize(course))
                return false;

            return true;
        }

        public virtual async Task<IActionResult> Details(string courseId)
        {
            var customer = _workContext.CurrentCustomer;

            var course = await _courseService.GetById(courseId);
            if (course == null)
                return InvokeHttp404();

            if (!await CheckPermission(course, customer))
                return InvokeHttp404();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false), _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                DisplayEditLink(Url.Action("Edit", "Course", new { id = course.Id, area = "Admin" }));

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewCourse", course.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewCourse"), course.Name);
            await _customerActionEventService.Viewed(customer, HttpContext.Request.Path.ToString(), Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");

            //model
            var model = await _mediator.Send(new GetCourse() {
                Course = course,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage
            });

            return View(model);
        }
        public virtual async Task<IActionResult> Lesson(string id)
        {
            var customer = _workContext.CurrentCustomer;

            var lesson = await _courseLessonService.GetById(id);
            if (lesson == null)
                return InvokeHttp404();

            var course = await _courseService.GetById(lesson.CourseId);
            if (course == null)
                return InvokeHttp404();

            if (!await CheckPermission(course, customer))
                return InvokeHttp404();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false), _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) && await _permissionService.Authorize(StandardPermissionProvider.ManageCourses, customer))
                DisplayEditLink(Url.Action("EditLesson", "Course", new { id = lesson.Id, area = "Admin" }));

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewLesson", lesson.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewLesson"), lesson.Name);
            await _customerActionEventService.Viewed(customer, HttpContext.Request.Path.ToString(), Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");

            //model
            var model = await _mediator.Send(new GetLesson() {
                Course = course,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Lesson = lesson
            });

            return View(model);
        }
        public virtual async Task<IActionResult> DownloadFile(string id)
        {
            var customer = _workContext.CurrentCustomer;

            var lesson = await _courseLessonService.GetById(id);
            if (lesson == null || string.IsNullOrEmpty(lesson.AttachmentId))
                return InvokeHttp404();

            var course = await _courseService.GetById(lesson.CourseId);
            if (course == null)
                return InvokeHttp404();

            if (!await CheckPermission(course, customer))
                return InvokeHttp404();

            var download = await _downloadService.GetDownloadById(lesson.AttachmentId);
            if (download == null)
                return Content("No download record found with the specified id");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //use stored data
            if (download.DownloadBinary == null)
                return Content(string.Format("Download data is not available any more. Download GD={0}", download.Id));

            string fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
            string contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : "application/octet-stream";
            return new FileContentResult(download.DownloadBinary, contentType) {
                FileDownloadName = fileName + download.Extension
            };
        }

        public virtual async Task<IActionResult> VideoFile(string id)
        {
            var customer = _workContext.CurrentCustomer;

            var lesson = await _courseLessonService.GetById(id);
            if (lesson == null || string.IsNullOrEmpty(lesson.VideoFile))
                return InvokeHttp404();

            var course = await _courseService.GetById(lesson.CourseId);
            if (course == null)
                return InvokeHttp404();

            if (!await CheckPermission(course, customer))
                return InvokeHttp404();

            var download = await _downloadService.GetDownloadById(lesson.VideoFile);
            if (download == null)
                return Content("No download record found with the specified id");

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //use stored data
            if (download.DownloadBinary == null)
                return Content(string.Format("Download data is not available any more. Download GD={0}", download.Id));

            string fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
            string contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : "video/mp4";
            return new FileContentResult(download.DownloadBinary, contentType) {
                FileDownloadName = fileName + download.Extension
            };
        }

        public virtual async Task<IActionResult> Approved(string id)
        {
            var customer = _workContext.CurrentCustomer;

            var lesson = await _courseLessonService.GetById(id);
            if (lesson == null)
                return Json(new { result = false });

            var course = await _courseService.GetById(lesson.CourseId);
            if (course == null)
                return Json(new { result = false });

            if (!await CheckPermission(course, customer))
                return Json(new { result = false });

            await _mediator.Send(new CourseLessonApprovedCommand() { Course = course, Lesson = lesson, Customer = _workContext.CurrentCustomer });

            return Json(new { result = true });
        }
    }
}
