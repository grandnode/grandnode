using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Web.Models.Course;
using MediatR;

namespace Grand.Web.Features.Models.Courses
{
    public class GetLesson : IRequest<LessonModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Course Course { get; set; }
        public CourseLesson Lesson { get; set; }
    }
}
