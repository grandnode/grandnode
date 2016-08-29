using System.Collections.Generic;
using Grand.Admin.Models.Directory;
using Grand.Web.Framework.Mvc;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Models.Shipping
{
    public partial class ShippingMethodRestrictionModel : BaseNopModel
    {
        public ShippingMethodRestrictionModel()
        {
            AvailableShippingMethods = new List<ShippingMethodModel>();
            AvailableCountries = new List<CountryModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Restricted = new Dictionary<string, IDictionary<string, bool>>();
            RestictedRole = new Dictionary<string, IDictionary<string, bool>>();
        }
        public IList<ShippingMethodModel> AvailableShippingMethods { get; set; }
        public IList<CountryModel> AvailableCountries { get; set; }
        public IList<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        //[country id] / [shipping method id] / [restricted]
        public IDictionary<string, IDictionary<string, bool>> Restricted { get; set; }
        public IDictionary<string, IDictionary<string, bool>> RestictedRole { get; set; }
    }
}