using Grand.Domain.Courses;
using Grand.Domain.Customers;
using MediatR;

namespace Grand.Web.Commands.Models.Courses
{
    public class CourseLessonApprovedCommand : IRequest<bool>
    {
        public Course Course { get; set; }
        public CourseLesson Lesson { get; set; }
        public Customer Customer { get; set; }
    }
}
