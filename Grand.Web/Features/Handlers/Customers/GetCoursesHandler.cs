using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Services.Courses;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetCoursesHandler : IRequestHandler<GetCourses, CoursesModel>
    {
        private readonly ICourseService _courseService;
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseActionService _courseActionService;

        public GetCoursesHandler(ICourseService courseService, 
            ICourseLevelService courseLevelService, 
            ICourseLessonService courseLessonService, 
            ICourseActionService courseActionService)
        {
            _courseService = courseService;
            _courseLevelService = courseLevelService;
            _courseLessonService = courseLessonService;
            _courseActionService = courseActionService;
        }

        public async Task<CoursesModel> Handle(GetCourses request, CancellationToken cancellationToken)
        {
            var model = new CoursesModel();
            model.CustomerId = request.Customer.Id;
            var courses = await _courseService.GetByCustomer(request.Customer, request.Store.Id);
            foreach (var item in courses)
            {
                var level = await _courseLevelService.GetById(item.LevelId);
                model.CourseList.Add(new CoursesModel.Course() {
                    Id = item.Id,
                    Name = item.Name,
                    SeName = item.SeName,
                    ShortDescription = item.ShortDescription,
                    Level = level?.Name,
                    Approved = await IsApprovedCourse(item, request.Customer),
                });
            }
            return model;
        }

        private async Task<bool> IsApprovedCourse(Course course, Customer customer)
        {
            var lessons = await _courseLessonService.GetByCourseId(course.Id);
            foreach (var item in lessons.Where(x => x.Published))
            {
                if (!await _courseActionService.CustomerLessonCompleted(customer.Id, item.Id))
                    return false;
            }
            return true;
        }
    }
}
