using Grand.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class CustomerActionTypeMappingExtensions
    {
        public static CustomerActionTypeModel ToModel(this CustomerActionType entity)
        {
            return entity.MapTo<CustomerActionType, CustomerActionTypeModel>();
        }
    }
}