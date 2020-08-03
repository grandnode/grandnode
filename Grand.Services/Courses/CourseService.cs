using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Customers;
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
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        public CourseService(IRepository<Course> courseRepository,
            IRepository<Order> orderRepository,
            CatalogSettings catalogSettings,
            IMediator mediator)
        {
            _courseRepository = courseRepository;
            _orderRepository = orderRepository;
            _catalogSettings = catalogSettings;
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

        public virtual async Task<IPagedList<Course>> GetAll(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from q in _courseRepository.Table
                        orderby q.DisplayOrder
                        select q;

            return await PagedList<Course>.Create(query, pageIndex, pageSize);
        }
        public virtual async Task<IList<Course>> GetByCustomer(Customer customer, string storeId)
        {
            var query = from c in _courseRepository.Table
                        select c;

            query = query.Where(c => c.Published);

            if ((!_catalogSettings.IgnoreAcl || (!string.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)))
            {
                if (!_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = customer.GetCustomerRoleIds();
                    query = from p in query
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                            select p;
                }
                if (!string.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    query = from p in query
                            where !p.LimitedToStores || p.Stores.Contains(storeId)
                            select p;
                }
            }

            //courses without assigned product
            var q1 = await query.Where(x => string.IsNullOrEmpty(x.ProductId)).ToListAsync();
            
            //get products from orders - paid/not deleted/for customer/store
            var pl = await _orderRepository.Table.Where(x => x.CustomerId == customer.Id && !x.Deleted
                            && x.PaymentStatusId == (int)Domain.Payments.PaymentStatus.Paid
                            && x.StoreId == storeId).SelectMany(x => x.OrderItems, (p, pr) => pr.ProductId).Distinct().ToListAsync();

            //courses assigned to products
            var q2 = await query.Where(x => pl.Contains(x.ProductId)).ToListAsync();

            return q1.Concat(q2).ToList();
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
