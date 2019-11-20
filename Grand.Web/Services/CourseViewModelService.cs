using Grand.Core;
using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Services.Courses;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Web.Extensions;
using Grand.Web.Interfaces;
using Grand.Web.Models.Course;
using Grand.Web.Models.Customer;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CourseViewModelService : ICourseViewModelService
    {
        private readonly ICourseService _courseService;
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseActionService _courseActionService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        public CourseViewModelService(ICourseService courseService,
            ICourseSubjectService courseSubjectService,
            ICourseLessonService courseLessonService,
            ICourseLevelService courseLevelService,
            ICourseActionService courseActionService,
            IOrderService orderService, IWorkContext workContext,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _courseService = courseService;
            _courseSubjectService = courseSubjectService;
            _courseLessonService = courseLessonService;
            _courseLevelService = courseLevelService;
            _courseActionService = courseActionService;
            _orderService = orderService;
            _workContext = workContext;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public virtual Task<Course> GetCourseById(string courseId)
        {
            return _courseService.GetById(courseId);
        }

        public virtual async Task<CoursesModel> GetCoursesByCustomer(Customer customer, string storeId)
        {
            var model = new CoursesModel();
            model.CustomerId = customer.Id;
            var courses = await _courseService.GetByCustomer(customer, storeId);
            foreach (var item in courses)
            {
                var level = await _courseLevelService.GetById(item.LevelId);
                model.CourseList.Add(new CoursesModel.Course() {
                    Id = item.Id,
                    Name = item.Name,
                    SeName = item.SeName,
                    ShortDescription = item.ShortDescription,
                    Level = level?.Name,
                    Approved = await IsApprovedCourse(item, customer),
                });
            }
            return model;
        }

        public virtual Task<CourseLesson> GetLessonById(string lessonId)
        {
            return _courseLessonService.GetById(lessonId);
        }

        public virtual async Task<bool> CheckOrder(Course course, Customer customer)
        {
            if (string.IsNullOrEmpty(course.ProductId))
                return true;

            var orders = await _orderService.SearchOrders(customerId: customer.Id, productId: course.ProductId, ps: Core.Domain.Payments.PaymentStatus.Paid);
            if (orders.TotalCount > 0)
                return true;

            return false;
        }
        public virtual async Task<bool> IsApprovedCourse(Course course, Customer customer)
        {
            var lessons = await _courseLessonService.GetByCourseId(course.Id);
            foreach (var item in lessons.Where(x=>x.Published))
            {
                if (!await _courseActionService.CustomerLessonCompleted(customer.Id, item.Id))
                    return false;
            }
            return true;
        }

        public virtual async Task<CourseModel> PrepareCourseModel(Course course)
        {
            var model = course.ToModel(_workContext.WorkingLanguage);
            model.Level = (await _courseLevelService.GetById(course.LevelId))?.Name;

            var pictureSize = _mediaSettings.CourseThumbPictureSize;
            var picture = await _pictureService.GetPictureById(course.PictureId);
            model.PictureUrl = await _pictureService.GetPictureUrl(picture, pictureSize);

            var subjects = await _courseSubjectService.GetByCourseId(course.Id);
            foreach (var item in subjects)
            {
                model.Subjects.Add(new CourseModel.Subject() {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayOrder = item.DisplayOrder
                });
            }

            var lessonPictureSize = _mediaSettings.LessonThumbPictureSize;
            var lessons = await _courseLessonService.GetByCourseId(course.Id);
            foreach (var item in lessons.Where(x => x.Published))
            {
                var lessonPicture = await _pictureService.GetPictureById(item.PictureId);
                var pictureUrl = await _pictureService.GetPictureUrl(lessonPicture, lessonPictureSize);
                var approved = await _courseActionService.CustomerLessonCompleted(_workContext.CurrentCustomer.Id, item.Id);

                model.Lessons.Add(new CourseModel.Lesson() {
                    Id = item.Id,
                    SubjectId = item.SubjectId,
                    Name = item.Name,
                    ShortDescription = item.ShortDescription,
                    DisplayOrder = item.DisplayOrder,
                    PictureUrl = pictureUrl,
                    Approved = approved
                });
            }
            model.Approved = !model.Lessons.Any(x => !x.Approved);
            return model;
        }
        public virtual async Task<LessonModel> PrepareLessonModel(Course course, CourseLesson lesson)
        {
            var model = new LessonModel();

            var modelCourse = course.ToModel(_workContext.WorkingLanguage);
            model.Id = lesson.Id;
            model.CourseId = modelCourse.Id;
            model.CourseDescription = modelCourse.Description;
            model.CourseName = modelCourse.Name;
            model.CourseSeName = modelCourse.SeName;
            model.MetaDescription = modelCourse.MetaDescription;
            model.MetaKeywords = model.MetaKeywords;
            model.MetaTitle = model.MetaTitle;
            model.Name = lesson.Name;
            model.ShortDescription = lesson.ShortDescription;
            model.Description = lesson.Description;
            model.GenericAttributes = lesson.GenericAttributes;
            model.CourseLevel = (await _courseLevelService.GetById(course.LevelId))?.Name;

            //prepare picture
            var picture = await _pictureService.GetPictureById(lesson.PictureId);
            model.PictureUrl = await _pictureService.GetPictureUrl(picture);

            model.Approved = await _courseActionService.CustomerLessonCompleted(_workContext.CurrentCustomer.Id, lesson.Id);
            if (!string.IsNullOrEmpty(lesson.AttachmentId))
                model.DownloadFile = true;

            if (!string.IsNullOrEmpty(lesson.VideoFile))
                model.VideoFile = true;

            return model;
        }
        public virtual async Task Approved(Course course, CourseLesson lesson, Customer customer)
        {
            var action = await _courseActionService.GetCourseAction(customer.Id, lesson.Id);
            if (action == null)
            {
                await _courseActionService.InsertAsync(new CourseAction() {
                    CustomerId = customer.Id,
                    CourseId = course.Id,
                    LessonId = lesson.Id,
                    Finished = true
                });
            }
            else
            {
                action.Finished = true;
                await _courseActionService.Update(action);

            }
        }

    }
}
