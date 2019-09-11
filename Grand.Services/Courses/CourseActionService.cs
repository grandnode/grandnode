using Grand.Core.Data;
using Grand.Core.Domain.Courses;
using Grand.Services.Events;
using MediatR;
using System;
using System.Threading.Tasks;

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
