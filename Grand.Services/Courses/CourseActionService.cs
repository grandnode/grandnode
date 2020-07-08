using Grand.Domain.Data;
using Grand.Domain.Courses;
using Grand.Services.Events;
using MediatR;
using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Services.Courses
{
    public class CourseActionService : ICourseActionService
    {
        private readonly IRepository<CourseAction> _courseActionRepository;
        private readonly IMediator _mediator;

        public CourseActionService(IRepository<CourseAction> courseActionRepository, IMediator mediator)
        {
            _courseActionRepository = courseActionRepository;
            _mediator = mediator;
        }

        public virtual Task<CourseAction> GetById(string id)
        {
            return _courseActionRepository.GetByIdAsync(id);
        }
        public virtual async Task<CourseAction> GetCourseAction(string customerId, string lessonId)
        {
            var query = from a in _courseActionRepository.Table
                              where a.CustomerId == customerId && a.LessonId == lessonId
                              select a;

            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<bool> CustomerLessonCompleted(string customerId, string lessonId)
        {
            var query = await (from a in _courseActionRepository.Table
                        where a.CustomerId == customerId && a.LessonId == lessonId
                        select a).FirstOrDefaultAsync();

            return query != null ? query.Finished : false;
        }

        public virtual async Task<CourseAction> InsertAsync(CourseAction courseAction)
        {
            if (courseAction == null)
                throw new ArgumentNullException("courseAction");

            await _courseActionRepository.InsertAsync(courseAction);

            //event notification
            await _mediator.EntityInserted(courseAction);

            return courseAction;
        }

        public virtual async Task<CourseAction> Update(CourseAction courseAction)
        {
            if (courseAction == null)
                throw new ArgumentNullException("courseAction");

            await _courseActionRepository.UpdateAsync(courseAction);

            //event notification
            await _mediator.EntityUpdated(courseAction);

            return courseAction;
        }
    }
}
