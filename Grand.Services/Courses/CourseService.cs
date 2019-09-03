using Grand.Core.Data;
using Grand.Core.Domain.Courses;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IMediator _mediator;

        public CourseService(IRepository<Course> courseRepository, IMediator mediator)
        {
            _courseRepository = courseRepository;
            _mediator = mediator;
        }

        public virtual async Task Delete(Course course)
        {
            if (course == null)
                throw new ArgumentNullException("course");

            await _courseRepository.DeleteAsync(course);

            //event notification
            await _mediator.EntityDeleted(course);
        }

        public virtual async Task<IList<Course>> GetAll()
        {
            return await _courseRepository.Table.ToListAsync();
        }

        public virtual Task<Course> GetById(string id)
        {
            return _courseRepository.GetByIdAsync(id);
        }

        public virtual async Task<Course> Insert(Course course)
        {
            if (course == null)
                throw new ArgumentNullException("course");

            await _courseRepository.InsertAsync(course);

            //event notification
            await _mediator.EntityInserted(course);

            return course;
        }

        public virtual async Task<Course> Update(Course course)
        {
            if (course == null)
                throw new ArgumentNullException("course");

            await _courseRepository.UpdateAsync(course);

            //event notification
            await _mediator.EntityUpdated(course);

            return course;
        }
    }
}
