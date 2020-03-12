using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
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
