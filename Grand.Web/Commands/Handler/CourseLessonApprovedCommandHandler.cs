using Grand.Core.Domain.Courses;
using Grand.Services.Courses;
using Grand.Web.Commands.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler
{
    public class CourseLessonApprovedCommandHandler : IRequestHandler<CourseLessonApprovedCommandModel, bool>
    {
        private readonly ICourseActionService _courseActionService;

        public CourseLessonApprovedCommandHandler(ICourseActionService courseActionService)
        {
            _courseActionService = courseActionService;
        }

        public async Task<bool> Handle(CourseLessonApprovedCommandModel request, CancellationToken cancellationToken)
        {
            var action = await _courseActionService.GetCourseAction(request.Customer.Id, request.Lesson.Id);
            if (action == null)
            {
                await _courseActionService.InsertAsync(new CourseAction() {
                    CustomerId = request.Customer.Id,
                    CourseId = request.Course.Id,
                    LessonId = request.Lesson.Id,
                    Finished = true
                });
            }
            else
            {
                action.Finished = true;
                await _courseActionService.Update(action);
            }
            return true;
        }
    }
}
