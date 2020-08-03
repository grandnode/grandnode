using Grand.Domain.Courses;
using Grand.Domain.Customers;
using MediatR;

namespace Grand.Web.Features.Models.Courses
{
    public class GetCheckOrder : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Course Course { get; set; }
    }
}
