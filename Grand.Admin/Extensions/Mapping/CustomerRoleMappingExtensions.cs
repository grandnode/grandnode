using Grand.Domain.Customers;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Extensions
{
    public static class CustomerRoleMappingExtensions
    {
        public static CustomerRoleModel ToModel(this CustomerRole entity)
        {
            return entity.MapTo<CustomerRole, CustomerRoleModel>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model)
        {
            return model.MapTo<CustomerRoleModel, CustomerRole>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model, CustomerRole destination)
        {
            return model.MapTo(destination);
        }
    }
}