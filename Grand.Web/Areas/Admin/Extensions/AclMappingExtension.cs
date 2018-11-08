using Grand.Core.Domain.Security;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.Models;
using Grand.Services.Customers;
using System.Linq;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class AclMappingExtension
    {
        public static void PrepareACLModel<T>(this T baseGrandEntityModel, IAclSupported aclMapping, bool excludeProperties, ICustomerService customerService)
            where T : BaseGrandEntityModel, IAclMappingModel
        {
            baseGrandEntityModel.AvailableCustomerRoles = customerService
               .GetAllCustomerRoles(true)
               .Select(s => new CustomerRoleModel { Id = s.Id, Name = s.Name })
               .ToList();
            if (!excludeProperties)
            {
                if (aclMapping != null)
                {
                    baseGrandEntityModel.SelectedCustomerRoleIds = aclMapping.CustomerRoles.ToArray();
                }
            }
        }
    }
}
