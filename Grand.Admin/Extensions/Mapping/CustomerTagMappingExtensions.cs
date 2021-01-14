using Grand.Domain.Customers;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Extensions
{
    public static class CustomerTagMappingExtensions
    {
        public static CustomerTagModel ToModel(this CustomerTag entity)
        {
            return entity.MapTo<CustomerTag, CustomerTagModel>();
        }

        public static CustomerTag ToEntity(this CustomerTagModel model)
        {
            return model.MapTo<CustomerTagModel, CustomerTag>();
        }

        public static CustomerTag ToEntity(this CustomerTagModel model, CustomerTag destination)
        {
            return model.MapTo(destination);
        }
    }
}