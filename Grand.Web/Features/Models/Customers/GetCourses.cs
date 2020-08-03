using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetCourses : IRequest<CoursesModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
