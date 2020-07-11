using Grand.Domain.Data;
using Grand.Domain.Courses;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public class CourseSubjectService : ICourseSubjectService
    {
        private readonly IRepository<CourseSubject> _courseSubjectRepository;
        private readonly IMediator _mediator;

        public CourseSubjectService(IRepository<CourseSubject> courseSubjectRepository, IMediator mediator)
        {
            _courseSubjectRepository = courseSubjectRepository;
            _mediator = mediator;
        }

        public virtual async Task Delete(CourseSubject courseSubject)
        {
            if (courseSubject == null)
                throw new ArgumentNullException("courseSubject");

            await _courseSubjectRepository.DeleteAsync(courseSubject);

            //event notification
            await _mediator.EntityDeleted(courseSubject);
        }

        public virtual async Task<IList<CourseSubject>> GetByCourseId(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
                throw new ArgumentNullException("courseId");

            var query = from c in _courseSubjectRepository.Table
                        where c.CourseId == courseId
                        orderby c.DisplayOrder
                        select c;

            return await query.ToListAsync();
        }

        public virtual Task<CourseSubject> GetById(string id)
        {
            return _courseSubjectRepository.GetByIdAsync(id);
        }

        public virtual async Task<CourseSubject> Insert(CourseSubject courseSubject)
        {
            if (courseSubject == null)
                throw new ArgumentNullException("courseSubject");

            await _courseSubjectRepository.InsertAsync(courseSubject);

            //event notification
            await _mediator.EntityInserted(courseSubject);

            return courseSubject;
        }

        public virtual async Task<CourseSubject> Update(CourseSubject courseSubject)
        {
            if (courseSubject == null)
                throw new ArgumentNullException("courseSubject");

            await _courseSubjectRepository.UpdateAsync(courseSubject);

            //event notification
            await _mediator.EntityUpdated(courseSubject);

            return courseSubject;
        }
    }
}
