using Grand.Domain.Customers;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Extensions
{
    public static class SalesEmployeeMappingExtensions
    {
        public static SalesEmployeeModel ToModel(this SalesEmployee entity)
        {
            return entity.MapTo<SalesEmployee, SalesEmployeeModel>();
        }

        public static SalesEmployee ToEntity(this SalesEmployeeModel model)
        {
            return model.MapTo<SalesEmployeeModel, SalesEmployee>();
        }

        public static SalesEmployee ToEntity(this SalesEmployeeModel model, SalesEmployee destination)
        {
            return model.MapTo(destination);
        }
    }
}