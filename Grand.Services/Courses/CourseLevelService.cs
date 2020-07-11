using Grand.Domain.Data;
using Grand.Domain.Courses;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public class CourseLevelService : ICourseLevelService
    {
        private readonly IRepository<CourseLevel> _courseLevelRepository;
        private readonly IMediator _mediator;

        public CourseLevelService(IRepository<CourseLevel> courseLevelRepository, IMediator mediator)
        {
            _courseLevelRepository = courseLevelRepository;
            _mediator = mediator;
        }

        public virtual async Task Delete(CourseLevel courseLevel)
        {
            if (courseLevel == null)
                throw new ArgumentNullException("courseLevel");

            await _courseLevelRepository.DeleteAsync(courseLevel);

            //event notification
            await _mediator.EntityDeleted(courseLevel);
        }

        public virtual async Task<IList<CourseLevel>> GetAll()
        {
            var query = from l in _courseLevelRepository.Table
                        orderby l.DisplayOrder
                        select l;

            return await query.ToListAsync();
        }

        public virtual Task<CourseLevel> GetById(string id)
        {
            return _courseLevelRepository.GetByIdAsync(id);
        }

        public virtual async Task<CourseLevel> Insert(CourseLevel courseLevel)
        {
            if (courseLevel == null)
                throw new ArgumentNullException("courseLevel");

            await _courseLevelRepository.InsertAsync(courseLevel);

            //event notification
            await _mediator.EntityInserted(courseLevel);

            return courseLevel;
        }

        public virtual async Task<CourseLevel> Update(CourseLevel courseLevel)
        {
            if (courseLevel == null)
                throw new ArgumentNullException("courseLevel");

            await _courseLevelRepository.UpdateAsync(courseLevel);

            //event notification
            await _mediator.EntityUpdated(courseLevel);

            return courseLevel;
        }
    }
}
